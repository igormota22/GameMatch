namespace GameMatch.UnitTests.Catalogo;

/// <summary>
/// Cria diretórios temporários com os três arquivos do snapshot. Mantém os
/// testes independentes do diretório <c>seed/</c> real do repositório.
/// </summary>
internal sealed class DiretorioSnapshot : IDisposable
{
    public DiretorioSnapshot()
    {
        Caminho = Path.Combine(
            Path.GetTempPath(),
            $"gamematch-snapshot-{Guid.NewGuid():N}");

        Directory.CreateDirectory(Caminho);
    }

    public string Caminho { get; }

    public static string CaminhoFixo =>
        Path.Combine(AppContext.BaseDirectory, "Catalogo", "Fixtures", "valido");

    public DiretorioSnapshot ComJogos(string json)
    {
        File.WriteAllText(Path.Combine(Caminho, "jogos.json"), json);
        return this;
    }

    public DiretorioSnapshot ComTags(string json)
    {
        File.WriteAllText(Path.Combine(Caminho, "tags.json"), json);
        return this;
    }

    public DiretorioSnapshot ComMetadados(string json)
    {
        File.WriteAllText(Path.Combine(Caminho, "metadados.json"), json);
        return this;
    }

    public DiretorioSnapshot Completo(
        string? jogos = null,
        string? tags = null,
        string? metadados = null)
    {
        ComJogos(jogos ?? Jogos());
        ComTags(tags ?? Tags());
        ComMetadados(metadados ?? Metadados());

        return this;
    }

    public static string Jogos(
        int id = 9001,
        string titulo = "Aurora Tactics",
        string genero = "Strategy",
        string plataforma = "PC (Windows)") => $$"""
        [
          {
            "id": {{id}},
            "title": "{{titulo}}",
            "thumbnail": "https://cdn.example/{{id}}.jpg",
            "short_description": "Descrição de {{titulo}}.",
            "game_url": "https://www.freetogame.com/g/{{id}}",
            "genre": "{{genero}}",
            "platform": "{{plataforma}}",
            "publisher": "Estúdio",
            "developer": "Estúdio",
            "release_date": "2024-01-01",
            "freetogame_profile_url": "https://www.freetogame.com/{{id}}"
          }
        ]
        """;

    public static string Tags(string json = """{ "tags": { "action": [9001] } }""") => json;

    public static string Metadados(
        int totalJogos = 1,
        int totalTags = 1,
        string coletadoEm = "2026-11-16T12:00:00+00:00",
        int versaoFormato = 1) => $$"""
        {
          "coletadoEm": "{{coletadoEm}}",
          "totalJogos": {{totalJogos}},
          "totalTags": {{totalTags}},
          "tagsIgnoradas": [],
          "origem": "https://www.freetogame.com/api/",
          "versaoFormato": {{versaoFormato}}
        }
        """;

    public void Dispose()
    {
        if (Directory.Exists(Caminho))
        {
            Directory.Delete(Caminho, recursive: true);
        }
    }
}
