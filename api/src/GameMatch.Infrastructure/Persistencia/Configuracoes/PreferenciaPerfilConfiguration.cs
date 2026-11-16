using GameMatch.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameMatch.Infrastructure.Persistencia.Configuracoes;

public sealed class PreferenciaPerfilConfiguration : IEntityTypeConfiguration<PreferenciaPerfil>
{
    public void Configure(EntityTypeBuilder<PreferenciaPerfil> builder)
    {
        builder.ToTable("perfis_preferencias");

        builder.HasKey(preferencia => new
        {
            preferencia.UsuarioId,
            preferencia.Tipo,
            preferencia.Valor
        });

        builder.Property(preferencia => preferencia.UsuarioId)
            .HasColumnName("usuario_id")
            .IsRequired();

        builder.Property(preferencia => preferencia.Tipo)
            .HasColumnName("tipo")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(preferencia => preferencia.Valor)
            .HasColumnName("valor")
            .HasMaxLength(100)
            .IsRequired();
    }
}
