using GameMatch.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameMatch.Infrastructure.Persistencia.Configuracoes;

public sealed class PerfilGamerConfiguration : IEntityTypeConfiguration<PerfilGamer>
{
    public void Configure(EntityTypeBuilder<PerfilGamer> builder)
    {
        builder.ToTable("perfis_gamer");

        builder.HasKey(perfil => perfil.UsuarioId);

        builder.Property(perfil => perfil.UsuarioId)
            .HasColumnName("usuario_id")
            .IsRequired();

        builder.Property(perfil => perfil.AtualizadoEm)
            .HasColumnName("atualizado_em")
            .IsRequired();

        builder.HasOne<Usuario>()
            .WithOne()
            .HasForeignKey<PerfilGamer>(perfil => perfil.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(perfil => perfil.Preferencias)
            .WithOne()
            .HasForeignKey(preferencia => preferencia.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
