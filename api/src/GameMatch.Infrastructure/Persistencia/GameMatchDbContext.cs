using GameMatch.Domain.Entidades;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GameMatch.Infrastructure.Persistencia;

public sealed class GameMatchDbContext(DbContextOptions<GameMatchDbContext> options)
    : IdentityDbContext<Usuario>(options)
{
    public DbSet<RegistroSistema> RegistrosSistema => Set<RegistroSistema>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GameMatchDbContext).Assembly);
    }
}
