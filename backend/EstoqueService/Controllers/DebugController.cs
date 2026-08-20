using Microsoft.AspNetCore.Mvc;

namespace EstoqueService.Controllers;


[ApiController]
[Route("api/debug")]
public class DebugController : ControllerBase
{
    public static bool SimularFalha { get; private set; } = false;

    [HttpPost("falhar")]
    public IActionResult Ativar()
    {
        SimularFalha = true;
        return Ok(new { mensagem = "Falha simulada ATIVADA no EstoqueService." });
    }
    
    [HttpPost("restaurar")]
    public IActionResult Desativar()
    {
        SimularFalha = false;
        return Ok(new { mensagem = "Falha simulada DESATIVADA no EstoqueService." });
    }
    
    [HttpGet("status")]
    public IActionResult Status() => Ok(new { simulandoFalha = SimularFalha });
}
