namespace GameMatch.IntegrationTests;

/// <summary>
/// Cria diretórios temporários com os três arquivos do snapshot para os testes
/// de sincronização, sem depender do <c>seed/</c> real do repositório.
/// </summary>
internal sealed class DiretorioSnapshot : IDisposable
{
    public DiretorioSnapshot()
    {
        Caminho = Path.Combine(
            Path.GetTempPath(),
            $"gamematch-sync-{Guid.NewGuid():N}");

        Directory.CreateDirectory(Caminho);
    }

    public string Caminho { get; }

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

    public void Dispose()
    {
        if (Directory.Exists(Caminho))
        {
            Directory.Delete(Caminho, recursive: true);
        }
    }
}
