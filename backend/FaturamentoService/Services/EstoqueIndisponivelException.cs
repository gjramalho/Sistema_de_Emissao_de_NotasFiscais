namespace FaturamentoService.Services;

public class EstoqueIndisponivelException : Exception
{
    public EstoqueIndisponivelException(string message, Exception? inner = null) : base(message, inner) { }
}
