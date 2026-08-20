namespace FaturamentoService.Models;

public class NotaFiscal
{
    public int Id { get; set; }
    public int Numero { get; set; }

    public StatusNotaFiscal Status { get; set; } = StatusNotaFiscal.Aberta;

    public DateTime CriadaEm { get; set; } = DateTime.UtcNow;
    public DateTime? FechadaEm { get; set; }

    public List<ItemNotaFiscal> Itens { get; set; } = new();
}
