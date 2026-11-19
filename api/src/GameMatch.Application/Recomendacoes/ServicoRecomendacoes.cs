using GameMatch.Application.Catalogo;
using GameMatch.Domain.Entidades;
using GameMatch.Domain.Recomendacoes;

namespace GameMatch.Application.Recomendacoes;

/// <summary>
/// Caso de uso das recomendações: valida perfil (409) e experiência (404)
/// antes de tocar no catálogo, calcula a compatibilidade em memória, ordena de
/// forma determinística (pontuação descrescente, empate por id) e aplica o
/// limite do topo (D2, D4, D5 e D7 do design).
/// </summary>
public sealed class ServicoRecomendacoes(
    IRecomendacaoRepositorio repositorio,
    OpcoesRecomendacoes opcoesRecomendacoes,
    OpcoesDescoberta opcoesDescoberta)
{
    public async Task<ResultadoRecomendacoes> ExecutarAsync(
        string usuarioId,
        string? experiencia,
        CancellationToken cancellationToken = default)
    {
        var perfil = await repositorio.ObterPreferenciasAsync(
            usuarioId,
            cancellationToken);

        // Perfil sem gêneros/plataformas não recomenda (CA-13), antes de
        // qualquer outra leitura.
        if (perfil is null || !perfil.EstaCompleto)
        {
            return ResultadoRecomendacoes.PerfilIncompleto();
        }

        var chave = string.IsNullOrWhiteSpace(experiencia)
            ? null
            : experiencia.Trim();

        OpcaoDescoberta? opcao = null;

        if (chave is not null)
        {
            // Chave fora de Descoberta:Opcoes → 404 antes de ler o catálogo,
            // evitando trabalho descartado (D5).
            opcao = opcoesDescoberta.PorChave(chave);

            if (opcao is null)
            {
                return ResultadoRecomendacoes.ExperienciaDesconhecida(chave);
            }
        }

        var catalogo = await repositorio.ObterCatalogoAsync(cancellationToken);

        var preferencias = new PreferenciasCalculo(
            Valores(perfil, TipoPreferencia.Genero),
            Valores(perfil, TipoPreferencia.Plataforma),
            Valores(perfil, TipoPreferencia.Estilo));

        var alvoExperiencia = opcao is null
            ? null
            : opcao.Generos.Concat(opcao.Tags).ToArray();

        var recomendacoes = catalogo
            .Select(jogo =>
            {
                var resultado = CalculadoraCompatibilidade.Calcular(
                    preferencias,
                    new CaracteristicasJogo(
                        jogo.Generos,
                        jogo.Plataformas,
                        jogo.Tags),
                    alvoExperiencia);

                return new RecomendacaoDto(
                    new JogoResumoDto(
                        jogo.Id,
                        jogo.Titulo,
                        jogo.Thumbnail,
                        jogo.Generos,
                        jogo.Plataformas,
                        jogo.Desenvolvedor),
                    jogo.Descricao,
                    resultado.Pontuacao,
                    resultado.Fatores
                        .Select(fator => new FatorRecomendacaoDto(
                            fator.Chave,
                            fator.Rotulo,
                            fator.Contribuicao,
                            fator.Correspondencias))
                        .ToArray());
            })
            .OrderByDescending(recomendacao => recomendacao.Pontuacao)
            .ThenBy(recomendacao => recomendacao.Jogo.Id)
            .Take(opcoesRecomendacoes.Top)
            .ToArray();

        return ResultadoRecomendacoes.Ok(recomendacoes);
    }

    /// <summary>
    /// Opções do Modo Descoberta (chave e rótulo) na ordem configurada, para o
    /// seletor da tela de recomendações (RF-14).
    /// </summary>
    public IReadOnlyList<OpcaoDescobertaDto> ListarOpcoes() =>
        opcoesDescoberta.Opcoes
            .Select(opcao => new OpcaoDescobertaDto(opcao.Chave, opcao.Rotulo))
            .ToArray();

    private static string[] Valores(PerfilGamer perfil, TipoPreferencia tipo) =>
        perfil.Preferencias
            .Where(preferencia => preferencia.Tipo == tipo)
            .Select(preferencia => preferencia.Valor)
            .ToArray();
}
