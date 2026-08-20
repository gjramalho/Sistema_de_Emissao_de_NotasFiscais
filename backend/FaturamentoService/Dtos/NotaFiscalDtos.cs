using FaturamentoService.Models;

namespace FaturamentoService.Dtos;

public record ItemNotaFiscalDto(int ProdutoId, string DescricaoProduto, int Quantidade);

public record NotaFiscalDto(
    int Id,
    int Numero,
    string Status,
    DateTime CriadaEm,
    DateTime? FechadaEm,
    List<ItemNotaFiscalDto> Itens);

public record CriarItemNotaDto(int ProdutoId, string DescricaoProduto, int Quantidade);

public record CriarNotaFiscalDto(List<CriarItemNotaDto> Itens);

public record ImprimirNotaResultDto(bool Sucesso, string Mensagem, NotaFiscalDto? Nota);

public static class NotaFiscalMapper
{
    // Converte a entidade persistida para o contrato enviado ao frontend.
    public static NotaFiscalDto ParaDto(NotaFiscal nota) => new(
        nota.Id,
        nota.Numero,
        nota.Status.ToString(),
        DateTime.SpecifyKind(nota.CriadaEm, DateTimeKind.Utc),
        nota.FechadaEm.HasValue
            ? DateTime.SpecifyKind(nota.FechadaEm.Value, DateTimeKind.Utc)
            : null,
        nota.Itens.Select(i => new ItemNotaFiscalDto(i.ProdutoId, i.DescricaoProduto, i.Quantidade)).ToList()
    );
}
