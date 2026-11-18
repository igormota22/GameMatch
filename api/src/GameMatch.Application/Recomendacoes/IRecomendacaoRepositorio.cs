using GameMatch.Domain.Entidades;

namespace GameMatch.Application.Recomendacoes;

/// <summary>
/// Contrato de leitura das recomendações: as preferências do usuário e o
/// catálogo completo com características agrupadas, para o cálculo em memória
/// (D2 do design da change).
/// </summary>
public interface IRecomendacaoRepositorio
{
    Task<PerfilGamer?> ObterPreferenciasAsync(
        string usuarioId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<JogoRecomendacao>> ObterCatalogoAsync(
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Jogo do catálogo com as características separadas em gêneros, plataformas
/// e tags (estilos), pronto para alimentar a calculadora de compatibilidade.
/// </summary>
public sealed record JogoRecomendacao(
    int Id,
    string Titulo,
    string Thumbnail,
    string Desenvolvedor,
    IReadOnlyList<string> Generos,
    IReadOnlyList<string> Plataformas,
    IReadOnlyList<string> Tags);
