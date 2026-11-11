using GameMatch.Domain.Entidades;
using Microsoft.EntityFrameworkCore;

namespace GameMatch.Infrastructure.Persistencia;

public sealed class GameMatchDbContext(DbContextOptions<GameMatchDbContext> options)
    : DbContext(options)
{
    public DbSet<RegistroSistema> RegistrosSistema => Set<RegistroSistema>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GameMatchDbContext).Assembly);
    }
}
