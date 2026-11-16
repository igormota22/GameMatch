using GameMatch.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameMatch.Infrastructure.Persistencia.Configuracoes;

public sealed class JogoConfiguration : IEntityTypeConfiguration<Jogo>
{
    public void Configure(EntityTypeBuilder<Jogo> builder)
    {
        builder.ToTable("jogos");

        builder.HasKey(jogo => jogo.Id);

        builder.Property(jogo => jogo.Id)
            .HasColumnName("id");

        builder.Property(jogo => jogo.IdExterno)
            .HasColumnName("id_externo")
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(jogo => jogo.IdExterno)
            .IsUnique();

        builder.Property(jogo => jogo.Titulo)
            .HasColumnName("titulo")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(jogo => jogo.Descricao)
            .HasColumnName("descricao")
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(jogo => jogo.Thumbnail)
            .HasColumnName("thumbnail")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(jogo => jogo.Desenvolvedor)
            .HasColumnName("desenvolvedor")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(jogo => jogo.UrlFonte)
            .HasColumnName("url_fonte")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(jogo => jogo.SincronizadoEm)
            .HasColumnName("sincronizado_em")
            .IsRequired();

        builder.HasMany(jogo => jogo.Caracteristicas)
            .WithOne()
            .HasForeignKey(caracteristica => caracteristica.JogoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
