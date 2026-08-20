using System.Text.Json;
using EstoqueService.Data;
using EstoqueService.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EstoqueService.Controllers;


[ApiController]
[Route("api/estoque")]
public class EstoqueController : ControllerBase
{
    private readonly EstoqueDbContext _db;
    private readonly ILogger<EstoqueController> _logger;

    public EstoqueController(EstoqueDbContext db, ILogger<EstoqueController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpPost("baixar")]
    public async Task<ActionResult<BaixaEstoqueResponseDto>> Baixar(
        [FromBody] BaixaEstoqueRequestDto request,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey)
    {

        if (DebugController.SimularFalha)
        {
            _logger.LogWarning("Falha simulada no EstoqueService (modo debug ativo).");
            return StatusCode(503, new { mensagem = "Serviço de Estoque indisponível (falha simulada)." });
        }

        if (request.Itens is null || request.Itens.Count == 0)
            return BadRequest(new { mensagem = "Nenhum item informado para baixa de estoque." });

        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            var existente = await _db.IdempotencyRecords.AsNoTracking()
                .FirstOrDefaultAsync(r => r.Key == idempotencyKey);

            if (existente is not null)
            {
                _logger.LogInformation("Idempotency-Key {Key} já processada. Retornando resultado em cache.", idempotencyKey);
                var cache = JsonSerializer.Deserialize<BaixaEstoqueResponseDto>(existente.ResultadoJson)!;
                return Ok(cache);
            }
        }

        var resultados = new List<ItemBaixaResultadoDto>();
        var sucessoGeral = true;

        foreach (var item in request.Itens)
        {
            var (sucesso, saldoAtual, mensagem) = await BaixarComRetryAsync(item.ProdutoId, item.Quantidade);
            resultados.Add(new ItemBaixaResultadoDto(item.ProdutoId, sucesso, saldoAtual, mensagem));
            if (!sucesso) sucessoGeral = false;
        }

        var response = new BaixaEstoqueResponseDto(sucessoGeral, resultados);

        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            _db.IdempotencyRecords.Add(new Models.IdempotencyRecord
            {
                Key = idempotencyKey,
                ResultadoJson = JsonSerializer.Serialize(response)
            });
            await _db.SaveChangesAsync();
        }

        return sucessoGeral ? Ok(response) : UnprocessableEntity(response);
    }

    private async Task<(bool sucesso, int saldoAtual, string? mensagem)> BaixarComRetryAsync(int produtoId, int quantidade)
    {
        const int maxTentativas = 3;

        for (var tentativa = 1; tentativa <= maxTentativas; tentativa++)
        {
            var produto = await _db.Produtos.FirstOrDefaultAsync(p => p.Id == produtoId);
            if (produto is null)
                return (false, 0, $"Produto {produtoId} não encontrado.");

            if (produto.Saldo < quantidade)
                return (false, produto.Saldo, $"Saldo insuficiente para o produto {produto.Codigo} (disponível: {produto.Saldo}, solicitado: {quantidade}).");

            produto.Saldo -= quantidade;

            try
            {
                await _db.SaveChangesAsync();
                return (true, produto.Saldo, null);
            }
            catch (DbUpdateConcurrencyException)
            {
                _logger.LogWarning(
                    "Conflito de concorrência ao debitar produto {ProdutoId}, tentativa {Tentativa}/{Max}.",
                    produtoId, tentativa, maxTentativas);

                _db.Entry(produto).State = EntityState.Detached;
            }
        }

        return (false, 0, "Não foi possível concluir a baixa por excesso de concorrência. Tente novamente.");
    }
}
