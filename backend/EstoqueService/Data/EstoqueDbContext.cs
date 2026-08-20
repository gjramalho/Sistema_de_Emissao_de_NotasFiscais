using EstoqueService.Models;
using Microsoft.EntityFrameworkCore;

namespace EstoqueService.Data;
public class EstoqueDbContext : DbContext
{
    public EstoqueDbContext(DbContextOptions<EstoqueDbContext> options) : base(options) { }
    public DbSet<Produto> Produtos => Set<Produto>();
    public DbSet<IdempotencyRecord> IdempotencyRecords => Set<IdempotencyRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Produto>(e =>
        {
           e.HasIndex(p => p.Codigo).IsUnique();
           e.Property(p => p.Codigo).IsRequired().HasMaxLength(50);
           e.Property(p => p.Descricao).IsRequired().HasMaxLength(200);

        });

        modelBuilder.Entity<IdempotencyRecord>(e =>
        {
            e.HasKey(i => i.Key);
        });

        modelBuilder.Entity<Produto>().HasData(
            new Produto {Id = 1, Codigo = "P001", Descricao = "Mouse sem fio", Saldo = 25},
            new Produto {Id = 2, Codigo = "P002", Descricao = "Teclado mecânico", Saldo = 10},
            new Produto {Id = 3, Codigo = "P003", Descricao = "Teclado Monitor 24\"", Saldo = 1}
        );
    }
}