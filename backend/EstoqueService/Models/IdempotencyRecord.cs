using System.Data;

namespace EstoqueService.Models;

public class IdempotencyRecord
{
    public string Key {get; set; } = string.Empty;
    public string ResultadoJson {get; set; } = string.Empty;
    public DateTime CriadoEm {get; set;} = DateTime.UtcNow;
    
}