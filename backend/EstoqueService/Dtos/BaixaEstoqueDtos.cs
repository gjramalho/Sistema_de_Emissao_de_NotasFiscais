namespace EstoqueService.Dtos;

public record ItemBaixaDto(int ProdutoId, int Quantidade);

public record BaixaEstoqueRequestDto(List<ItemBaixaDto> Itens);

public record ItemBaixaResultadoDto(int ProdutoId, bool Sucesso, int SaldoAtual, string? Mensagem);

public record BaixaEstoqueResponseDto(bool Sucesso, List<ItemBaixaResultadoDto> Itens);
