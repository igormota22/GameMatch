using GameMatch.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameMatch.Infrastructure.Persistencia.Configuracoes;

public sealed class AvaliacaoConfiguration : IEntityTypeConfiguration<Avaliacao>
{
    public void Configure(EntityTypeBuilder<Avaliacao> builder)
    {
        builder.ToTable("avaliacoes", tabela =>
            tabela.HasCheckConstraint(
                "ck_avaliacoes_nota",
                $"nota BETWEEN {Avaliacao.NotaMinima} AND {Avaliacao.NotaMaxima}"));

        builder.HasKey(avaliacao => new
        {
            avaliacao.UsuarioId,
            avaliacao.JogoId
        });

        builder.Property(avaliacao => avaliacao.UsuarioId)
            .HasColumnName("usuario_id")
            .IsRequired();

        builder.Property(avaliacao => avaliacao.JogoId)
            .HasColumnName("jogo_id")
            .IsRequired();

        builder.Property(avaliacao => avaliacao.Nota)
            .HasColumnName("nota")
            .IsRequired();

        builder.Property(avaliacao => avaliacao.AtualizadaEm)
            .HasColumnName("atualizada_em")
            .IsRequired();

        // Listas por usuário e exclusão nas recomendações (INV-06).
        builder.HasIndex(avaliacao => avaliacao.UsuarioId);

        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(avaliacao => avaliacao.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Jogo>()
            .WithMany()
            .HasForeignKey(avaliacao => avaliacao.JogoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
