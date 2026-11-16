namespace GameMatch.Application.Catalogo;

/// <summary>
/// Dados completos de um jogo, incluindo as características e a URL da fonte.
/// </summary>
public sealed record JogoDetalheDto(
    int Id,
    string Titulo,
    string Descricao,
    string Thumbnail,
    string Desenvolvedor,
    string UrlFonte,
    IReadOnlyList<string> Generos,
    IReadOnlyList<string> Plataformas,
    IReadOnlyList<string> Tags);
