using GameMatch.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameMatch.Infrastructure.Persistencia.Configuracoes;

public sealed class JogoCaracteristicaConfiguration
    : IEntityTypeConfiguration<JogoCaracteristica>
{
    public void Configure(EntityTypeBuilder<JogoCaracteristica> builder)
    {
        builder.ToTable("jogos_caracteristicas");

        builder.HasKey(caracteristica => new
        {
            caracteristica.JogoId,
            caracteristica.Tipo,
            caracteristica.Valor
        });

        builder.Property(caracteristica => caracteristica.JogoId)
            .HasColumnName("jogo_id")
            .IsRequired();

        builder.Property(caracteristica => caracteristica.Tipo)
            .HasColumnName("tipo")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(caracteristica => caracteristica.Valor)
            .HasColumnName("valor")
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(caracteristica => new
        {
            caracteristica.Tipo,
            caracteristica.Valor
        });
    }
}
