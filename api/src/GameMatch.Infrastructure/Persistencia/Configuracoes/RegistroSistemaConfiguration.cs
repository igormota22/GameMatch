using GameMatch.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameMatch.Infrastructure.Persistencia.Configuracoes;

public sealed class RegistroSistemaConfiguration : IEntityTypeConfiguration<RegistroSistema>
{
    public void Configure(EntityTypeBuilder<RegistroSistema> builder)
    {
        builder.ToTable("registros_sistema");

        builder.HasKey(registro => registro.Chave);

        builder.Property(registro => registro.Chave)
            .HasColumnName("chave")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(registro => registro.Valor)
            .HasColumnName("valor")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(registro => registro.AtualizadoEm)
            .HasColumnName("atualizado_em")
            .IsRequired();
    }
}
