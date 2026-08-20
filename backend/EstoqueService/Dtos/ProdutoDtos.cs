namespace EstoqueService.Dtos;

public record ProdutoDto(int Id, string Codigo, string Descricao, int Saldo);

public record CriarProdutoDto(string Codigo, string Descricao, int Saldo);

public record AtualizarProdutoDto(string Descricao, int Saldo);
