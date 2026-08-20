using FaturamentoService.Data;
using FaturamentoService.Dtos;
using FaturamentoService.Models;
using FaturamentoService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FaturamentoService.Controllers;

[ApiController]
[Route("api/notas-fiscais")]
public class NotasFiscaisController : ControllerBase
{
    private readonly FaturamentoDbContext _db;
    private readonly IEstoqueClient _estoqueClient;
    private readonly ILogger<NotasFiscaisController> _logger;

    public NotasFiscaisController(FaturamentoDbContext db, IEstoqueClient estoqueClient, ILogger<NotasFiscaisController> logger)
    {
        _db = db;
        _estoqueClient = estoqueClient;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<NotaFiscalDto>>> Listar()
    {
        var notas = await _db.NotasFiscais
            .AsNoTracking()
            .Include(n => n.Itens)
            .OrderByDescending(n => n.Numero)
            .ToListAsync();

        return Ok(notas.Select(NotaFiscalMapper.ParaDto));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<NotaFiscalDto>> ObterPorId(int id)
    {
        var nota = await _db.NotasFiscais.AsNoTracking().Include(n => n.Itens).FirstOrDefaultAsync(n => n.Id == id);
        if (nota is null) return NotFound(new { mensagem = $"Nota fiscal {id} não encontrada." });
        return Ok(NotaFiscalMapper.ParaDto(nota));
    }

    [HttpPost]
    public async Task<ActionResult<NotaFiscalDto>> Criar([FromBody] CriarNotaFiscalDto dto)
    {
        if (dto.Itens is null || dto.Itens.Count == 0)
            return BadRequest(new { mensagem = "A nota fiscal precisa ter ao menos um produto." });

        if (dto.Itens.Any(i => i.Quantidade <= 0))
            return BadRequest(new { mensagem = "A quantidade de cada item deve ser maior que zero." });

        var maiorNumero = await _db.NotasFiscais.Select(n => (int?)n.Numero).MaxAsync() ?? 0;

        var nota = new NotaFiscal
        {
            Numero = maiorNumero + 1,
            Status = StatusNotaFiscal.Aberta,
            Itens = dto.Itens.Select(i => new ItemNotaFiscal
            {
                ProdutoId = i.ProdutoId,
                DescricaoProduto = i.DescricaoProduto,
                Quantidade = i.Quantidade
            }).ToList()
        };

        _db.NotasFiscais.Add(nota);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Nota fiscal {Numero} criada com {QtdItens} item(ns).", nota.Numero, nota.Itens.Count);

        return CreatedAtAction(nameof(ObterPorId), new { id = nota.Id }, NotaFiscalMapper.ParaDto(nota));
    }

    [HttpPost("{id:int}/imprimir")]
    public async Task<ActionResult<ImprimirNotaResultDto>> Imprimir(int id)
    {
        var nota = await _db.NotasFiscais.Include(n => n.Itens).FirstOrDefaultAsync(n => n.Id == id);
        if (nota is null) return NotFound(new { mensagem = $"Nota fiscal {id} não encontrada." });

        if (nota.Status != StatusNotaFiscal.Aberta)
        {
            return BadRequest(new ImprimirNotaResultDto(
                false, $"Não é possível imprimir: a nota {nota.Numero} já está '{nota.Status}'.", NotaFiscalMapper.ParaDto(nota)));
        }

        var itensParaBaixa = nota.Itens
            .Select(i => new ItemBaixaRequest(i.ProdutoId, i.Quantidade))
            .ToList();

        BaixaEstoqueResultado resultadoEstoque;
        try
        {
            var idempotencyKey = $"nota-{nota.Id}-imprimir";
            resultadoEstoque = await _estoqueClient.BaixarEstoqueAsync(idempotencyKey, itensParaBaixa);
        }
        catch (EstoqueIndisponivelException ex)
        {
            _logger.LogWarning(ex, "Falha ao imprimir nota {Numero}: Estoque indisponível.", nota.Numero);
            return StatusCode(503, new ImprimirNotaResultDto(false, ex.Message, NotaFiscalMapper.ParaDto(nota)));
        }

        if (!resultadoEstoque.Sucesso)
        {
            var motivos = string.Join(" | ", resultadoEstoque.Itens.Where(i => !i.Sucesso).Select(i => i.Mensagem));
            return UnprocessableEntity(new ImprimirNotaResultDto(
                false, $"Não foi possível debitar o estoque: {motivos}", NotaFiscalMapper.ParaDto(nota)));
        }

        nota.Status = StatusNotaFiscal.Fechada;
        nota.FechadaEm = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Nota fiscal {Numero} impressa e fechada com sucesso.", nota.Numero);

        return Ok(new ImprimirNotaResultDto(true, "Nota fiscal impressa e fechada com sucesso.", NotaFiscalMapper.ParaDto(nota)));
    }
}
