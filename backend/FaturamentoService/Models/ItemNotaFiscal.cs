namespace FaturamentoService.Models;

public class ItemNotaFiscal
{
    public int Id { get; set; }
    public int NotaFiscalId { get; set; }
    public NotaFiscal? NotaFiscal { get; set; }

    public int ProdutoId { get; set; }

    public string DescricaoProduto { get; set; } = string.Empty;
    public int Quantidade { get; set; }
}
