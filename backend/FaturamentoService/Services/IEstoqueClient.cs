using FaturamentoService.Dtos;

namespace FaturamentoService.Services;

public record ItemBaixaRequest(int ProdutoId, int Quantidade);
public record ItemBaixaResultado(int ProdutoId, bool Sucesso, int SaldoAtual, string? Mensagem);
public record BaixaEstoqueResultado(bool Sucesso, List<ItemBaixaResultado> Itens);

public interface IEstoqueClient
{
    Task<BaixaEstoqueResultado> BaixarEstoqueAsync(string idempotencyKey, List<ItemBaixaRequest> itens, CancellationToken ct = default);
}