using System.Net.Http.Json;
using Polly.CircuitBreaker;

namespace FaturamentoService.Services;

public class EstoqueClient : IEstoqueClient
{
    private readonly HttpClient _http;
    private readonly ILogger<EstoqueClient> _logger;

    public EstoqueClient(HttpClient http, ILogger<EstoqueClient> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<BaixaEstoqueResultado> BaixarEstoqueAsync(
        string idempotencyKey, List<ItemBaixaRequest> itens, CancellationToken ct = default)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/estoque/baixar")
            {
                Content = JsonContent.Create(new { Itens = itens })
            };
            request.Headers.Add("Idempotency-Key", idempotencyKey);

            var response = await _http.SendAsync(request, ct);

            if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
            {
                throw new EstoqueIndisponivelException(
                    "O Serviço de Estoque está indisponível no momento. A nota fiscal continua Aberta; tente imprimir novamente em instantes.");
            }

            var resultado = await response.Content.ReadFromJsonAsync<BaixaEstoqueResultado>(cancellationToken: ct);

            if (resultado is null)
                throw new EstoqueIndisponivelException("Resposta inesperada do Serviço de Estoque.");

            return resultado;
        }
        catch (EstoqueIndisponivelException)
        {
            throw;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or BrokenCircuitException)
        {
            _logger.LogError(ex, "Falha de comunicação com o Serviço de Estoque.");
            throw new EstoqueIndisponivelException(
                "Não foi possível falar com o Serviço de Estoque (serviço fora do ar, lento, ou circuito aberto após falhas repetidas). A nota fiscal continua Aberta.",
                ex);
        }
    }
}
