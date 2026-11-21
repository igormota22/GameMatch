using GameMatch.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameMatch.Infrastructure.Persistencia.Configuracoes;

public sealed class FavoritoConfiguration : IEntityTypeConfiguration<Favorito>
{
    public void Configure(EntityTypeBuilder<Favorito> builder)
    {
        builder.ToTable("favoritos");

        builder.HasKey(favorito => new
        {
            favorito.UsuarioId,
            favorito.JogoId
        });

        builder.Property(favorito => favorito.UsuarioId)
            .HasColumnName("usuario_id")
            .IsRequired();

        builder.Property(favorito => favorito.JogoId)
            .HasColumnName("jogo_id")
            .IsRequired();

        builder.Property(favorito => favorito.CriadoEm)
            .HasColumnName("criado_em")
            .IsRequired();

        // Listas por usuário e exclusão nas recomendações (INV-06).
        builder.HasIndex(favorito => favorito.UsuarioId);

        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(favorito => favorito.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Jogo>()
            .WithMany()
            .HasForeignKey(favorito => favorito.JogoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
