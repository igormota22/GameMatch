using GameMatch.Domain.Entidades;
using GameMatch.Infrastructure.Persistencia;
using GameMatch.Seed.Snapshot;
using Microsoft.EntityFrameworkCore;

namespace GameMatch.Seed.Sincronizacao;

/// <summary>
/// Sincronização idempotente do catálogo: upsert por identificador externo e
/// substituição das características, tudo em uma transação.
/// </summary>
public sealed class SincronizadorCatalogo(GameMatchDbContext contexto)
{
    public async Task<ResumoSincronizacao> SincronizarAsync(
        SnapshotCatalogo snapshot,
        CancellationToken cancellationToken = default)
    {
        var tagsPorJogo = AgruparTags(snapshot, out var referenciasIgnoradas);

        await using var transacao = await contexto.Database
            .BeginTransactionAsync(cancellationToken);

        var idsExternos = snapshot.Jogos
            .Select(jogo => jogo.IdExterno)
            .ToArray();

        var existentes = await contexto.Jogos
            .Include(jogo => jogo.Caracteristicas)
            .Where(jogo => idsExternos.Contains(jogo.IdExterno))
            .ToDictionaryAsync(
                jogo => jogo.IdExterno,
                StringComparer.Ordinal,
                cancellationToken);

        contexto.JogosCaracteristicas.RemoveRange(
            existentes.Values.SelectMany(jogo => jogo.Caracteristicas));

        foreach (var jogo in existentes.Values)
        {
            jogo.Caracteristicas.Clear();
        }

        var inseridos = 0;
        var atualizados = 0;

        foreach (var jogoSnapshot in snapshot.Jogos)
        {
            if (existentes.TryGetValue(jogoSnapshot.IdExterno, out var existente))
            {
                Atualizar(existente, jogoSnapshot, snapshot.Metadados.ColetadoEm);
                atualizados++;
            }
            else
            {
                var novo = Criar(jogoSnapshot, snapshot.Metadados.ColetadoEm);

                existentes[jogoSnapshot.IdExterno] = novo;
                contexto.Jogos.Add(novo);
                inseridos++;
            }
        }

        // Primeiro aplica exclusões e atualizações; depois recria as
        // características para não reutilizar chaves ainda marcadas para remoção.
        await contexto.SaveChangesAsync(cancellationToken);

        foreach (var jogoSnapshot in snapshot.Jogos)
        {
            AdicionarCaracteristicas(
                existentes[jogoSnapshot.IdExterno],
                jogoSnapshot,
                tagsPorJogo);
        }

        await contexto.SaveChangesAsync(cancellationToken);
        await transacao.CommitAsync(cancellationToken);

        return new ResumoSincronizacao(inseridos, atualizados, referenciasIgnoradas);
    }

    private static Dictionary<string, IReadOnlyList<string>> AgruparTags(
        SnapshotCatalogo snapshot,
        out int referenciasIgnoradas)
    {
        var porJogo = snapshot.Jogos.ToDictionary(
            jogo => jogo.IdExterno,
            _ => new List<string>(),
            StringComparer.Ordinal);

        var ignoradas = 0;

        foreach (var (tag, ids) in snapshot.Tags)
        {
            foreach (var id in ids)
            {
                if (porJogo.TryGetValue(id, out var tags))
                {
                    tags.Add(tag);
                }
                else
                {
                    ignoradas++;
                }
            }
        }

        referenciasIgnoradas = ignoradas;

        return porJogo.ToDictionary(
            par => par.Key,
            par => (IReadOnlyList<string>)par.Value,
            StringComparer.Ordinal);
    }

    private static Jogo Criar(JogoSnapshot snapshot, DateTimeOffset coletadoEm) =>
        new()
        {
            IdExterno = snapshot.IdExterno,
            Titulo = snapshot.Titulo,
            Descricao = snapshot.Descricao,
            Thumbnail = snapshot.Thumbnail,
            Desenvolvedor = snapshot.Desenvolvedor,
            UrlFonte = snapshot.UrlFonte,
            SincronizadoEm = coletadoEm
        };

    private static void Atualizar(
        Jogo jogo,
        JogoSnapshot snapshot,
        DateTimeOffset coletadoEm)
    {
        jogo.Titulo = snapshot.Titulo;
        jogo.Descricao = snapshot.Descricao;
        jogo.Thumbnail = snapshot.Thumbnail;
        jogo.Desenvolvedor = snapshot.Desenvolvedor;
        jogo.UrlFonte = snapshot.UrlFonte;
        jogo.SincronizadoEm = coletadoEm;
    }

    private static void AdicionarCaracteristicas(
        Jogo jogo,
        JogoSnapshot snapshot,
        IReadOnlyDictionary<string, IReadOnlyList<string>> tagsPorJogo)
    {
        jogo.Caracteristicas.Add(new JogoCaracteristica
        {
            JogoId = jogo.Id,
            Tipo = TipoCaracteristica.Genero,
            Valor = snapshot.Genero
        });

        jogo.Caracteristicas.Add(new JogoCaracteristica
        {
            JogoId = jogo.Id,
            Tipo = TipoCaracteristica.Plataforma,
            Valor = snapshot.Plataforma
        });

        foreach (var tag in tagsPorJogo[snapshot.IdExterno]
                     .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            jogo.Caracteristicas.Add(new JogoCaracteristica
            {
                JogoId = jogo.Id,
                Tipo = TipoCaracteristica.Estilo,
                Valor = tag
            });
        }
    }
}

/// <summary>
/// Resumo da sincronização exibido pelo comando.
/// </summary>
public sealed record ResumoSincronizacao(
    int Inseridos,
    int Atualizados,
    int ReferenciasIgnoradas);
