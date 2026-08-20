using EstoqueService.Data;
using EstoqueService.Dtos;
using EstoqueService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EstoqueService.Controllers;

[ApiController]
[Route("api/produtos")]
public class ProdutosController : ControllerBase
{
    private readonly EstoqueDbContext _db;
    private readonly ILogger<ProdutosController> _logger;

    public ProdutosController(EstoqueDbContext db, ILogger<ProdutosController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProdutoDto>>> Listar([FromQuery] int? saldoMinimo)
    {
        var query = _db.Produtos.AsNoTracking().AsQueryable();

        if (saldoMinimo.HasValue)
        {
            query = query.Where(p => p.Saldo >= saldoMinimo.Value);
        }

        var produtos = await query
            .OrderBy(p => p.Codigo)
            .Select(p => new ProdutoDto(p.Id, p.Codigo, p.Descricao, p.Saldo))
            .ToListAsync();

        return Ok(produtos);
    }


    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProdutoDto>> ObterPorId(int id)
    {
        var produto = await _db.Produtos.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        if (produto is null) return NotFound(new { mensagem = $"Produto {id} não encontrado." });

        return Ok(new ProdutoDto(produto.Id, produto.Codigo, produto.Descricao, produto.Saldo));
    }

    [HttpPost]
    public async Task<ActionResult<ProdutoDto>> Criar([FromBody] CriarProdutoDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Codigo) || string.IsNullOrWhiteSpace(dto.Descricao))
            return BadRequest(new { mensagem = "Código e Descrição são obrigatórios." });

        if (dto.Saldo < 0)
            return BadRequest(new { mensagem = "Saldo não pode ser negativo." });

        var codigoExiste = await _db.Produtos.AnyAsync(p => p.Codigo == dto.Codigo);
        if (codigoExiste)
            return Conflict(new { mensagem = $"Já existe um produto com o código '{dto.Codigo}'." });

        var produto = new Produto
        {
            Codigo = dto.Codigo.Trim(),
            Descricao = dto.Descricao.Trim(),
            Saldo = dto.Saldo
        };

        _db.Produtos.Add(produto);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Produto {Codigo} cadastrado com saldo inicial {Saldo}", produto.Codigo, produto.Saldo);

        var resultado = new ProdutoDto(produto.Id, produto.Codigo, produto.Descricao, produto.Saldo);
        return CreatedAtAction(nameof(ObterPorId), new { id = produto.Id }, resultado);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProdutoDto>> Atualizar(int id, [FromBody] AtualizarProdutoDto dto)
    {
        var produto = await _db.Produtos.FirstOrDefaultAsync(p => p.Id == id);
        if (produto is null) return NotFound(new { mensagem = $"Produto {id} não encontrado." });

        produto.Descricao = dto.Descricao.Trim();
        produto.Saldo = dto.Saldo;

        await _db.SaveChangesAsync();
        return Ok(new ProdutoDto(produto.Id, produto.Codigo, produto.Descricao, produto.Saldo));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Remover(int id)
    {
        var produto = await _db.Produtos.FirstOrDefaultAsync(p => p.Id == id);
        if (produto is null) return NotFound(new { mensagem = $"Produto {id} não encontrado." });

        _db.Produtos.Remove(produto);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
