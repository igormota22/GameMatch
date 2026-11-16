namespace GameMatch.Application.Catalogo;

/// <summary>
/// Resumo de um jogo para a listagem do catálogo e para as recomendações.
/// </summary>
public sealed record JogoResumoDto(
    int Id,
    string Titulo,
    string Thumbnail,
    IReadOnlyList<string> Generos,
    IReadOnlyList<string> Plataformas,
    string Desenvolvedor);
