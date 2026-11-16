namespace GameMatch.Seed.Snapshot;

/// <summary>
/// Jogo do snapshot já validado e pronto para a sincronização.
/// </summary>
public sealed record JogoSnapshot(
    int Id,
    string IdExterno,
    string Titulo,
    string Descricao,
    string Thumbnail,
    string Desenvolvedor,
    string UrlFonte,
    string Genero,
    string Plataforma);

/// <summary>
/// Snapshot validado do catálogo.
/// </summary>
public sealed record SnapshotCatalogo(
    IReadOnlyList<JogoSnapshot> Jogos,
    IReadOnlyDictionary<string, IReadOnlyList<string>> Tags,
    MetadadosSnapshot Metadados);

/// <summary>
/// Indica que o snapshot está ausente ou não respeita o formato esperado. Nesse
/// caso a sincronização não altera o banco.
/// </summary>
public sealed class FormatoSnapshotInvalidoException(string mensagem)
    : Exception(mensagem);
