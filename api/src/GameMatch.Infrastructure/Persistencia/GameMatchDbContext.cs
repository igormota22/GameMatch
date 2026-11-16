using GameMatch.Domain.Entidades;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GameMatch.Infrastructure.Persistencia;

public sealed class GameMatchDbContext(DbContextOptions<GameMatchDbContext> options)
    : IdentityDbContext<Usuario>(options)
{
    public DbSet<RegistroSistema> RegistrosSistema => Set<RegistroSistema>();

    public DbSet<PerfilGamer> PerfisGamer => Set<PerfilGamer>();

    public DbSet<PreferenciaPerfil> PerfisPreferencias => Set<PreferenciaPerfil>();

    public DbSet<Jogo> Jogos => Set<Jogo>();

    public DbSet<JogoCaracteristica> JogosCaracteristicas => Set<JogoCaracteristica>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GameMatchDbContext).Assembly);
    }
}
