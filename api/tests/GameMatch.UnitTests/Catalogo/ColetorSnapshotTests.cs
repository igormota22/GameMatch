using System.Net;
using System.Text;
using GameMatch.Seed.Coleta;

namespace GameMatch.UnitTests.Catalogo;

public sealed class ColetorSnapshotTests
{
    private const string BaseUrl = "https://api.exemplo/";

    [Fact]
    public async Task Coletar_grava_snapshot_e_ignora_tags_com_erro_ou_sem_jogos()
    {
        var manipulador = new ManipuladorFake(requisicao =>
        {
            var caminho = requisicao.RequestUri!.PathAndQuery;

            return caminho switch
            {
                "/games" => Json(JogosDaFonte),
                "/filter?tag=action" => Json("""
                    [ { "id": 10, "title": "Aurora", "genre": "Strategy", "platform": "PC (Windows)" } ]
                    """),
                "/filter?tag=vazia" => Json("[]"),
                "/filter?tag=quebrada" => new HttpResponseMessage(
                    HttpStatusCode.InternalServerError),
                _ => new HttpResponseMessage(HttpStatusCode.NotFound)
            };
        });

        var diretorio = CriarDiretorio();

        try
        {
            var avisos = new List<string>();
            var resumo = await new ColetorSnapshot(new HttpClient(manipulador))
                .ColetarAsync(
                    Opcoes(["action", "vazia", "quebrada"]),
                    diretorio,
                    avisos.Add);

            Assert.Equal(2, resumo.TotalJogos);
            Assert.Equal(1, resumo.TotalTags);
            Assert.Equal(["vazia", "quebrada"], resumo.TagsIgnoradas);
            Assert.Equal(2, avisos.Count);

            Assert.True(File.Exists(Path.Combine(diretorio, "jogos.json")));
            Assert.True(File.Exists(Path.Combine(diretorio, "tags.json")));
            Assert.True(File.Exists(Path.Combine(diretorio, "metadados.json")));
            Assert.Empty(Directory.GetFiles(diretorio, "*.tmp"));

            var jogos = await File.ReadAllTextAsync(
                Path.Combine(diretorio, "jogos.json"));
            Assert.Contains("\"Aurora", jogos);

            var tags = await File.ReadAllTextAsync(
                Path.Combine(diretorio, "tags.json"));
            Assert.Contains("\"action\"", tags);
            Assert.DoesNotContain("\"vazia\"", tags);
            Assert.DoesNotContain("\"quebrada\"", tags);

            var metadados = await File.ReadAllTextAsync(
                Path.Combine(diretorio, "metadados.json"));
            Assert.Contains("\"totalJogos\": 2", metadados);
            Assert.Contains("\"totalTags\": 1", metadados);
            Assert.Contains("\"vazia\"", metadados);

            Assert.Equal(
                ["/games", "/filter?tag=action", "/filter?tag=vazia", "/filter?tag=quebrada"],
                manipulador.Caminhos);
        }
        finally
        {
            Directory.Delete(diretorio, recursive: true);
        }
    }

    [Fact]
    public async Task Coletar_falha_de_rede_mantem_snapshot_anterior()
    {
        var manipulador = new ManipuladorFake(_ =>
            throw new HttpRequestException("fonte indisponível"));

        var diretorio = CriarDiretorio();

        try
        {
            var metadados = Path.Combine(diretorio, "metadados.json");
            await File.WriteAllTextAsync(metadados, "snapshot anterior");

            await Assert.ThrowsAsync<HttpRequestException>(() =>
                new ColetorSnapshot(new HttpClient(manipulador))
                    .ColetarAsync(Opcoes(["action"]), diretorio));

            Assert.Equal("snapshot anterior", await File.ReadAllTextAsync(metadados));
            Assert.False(File.Exists(Path.Combine(diretorio, "jogos.json")));
            Assert.False(File.Exists(Path.Combine(diretorio, "tags.json")));
        }
        finally
        {
            Directory.Delete(diretorio, recursive: true);
        }
    }

    [Fact]
    public async Task Coletar_sem_jogos_falha_sem_gravar_snapshot()
    {
        var manipulador = new ManipuladorFake(_ => Json("[]"));
        var diretorio = CriarDiretorio();

        try
        {
            var excecao = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                new ColetorSnapshot(new HttpClient(manipulador))
                    .ColetarAsync(Opcoes(["action"]), diretorio));

            Assert.Contains("não retornou jogos", excecao.Message);
            Assert.False(File.Exists(Path.Combine(diretorio, "jogos.json")));
        }
        finally
        {
            Directory.Delete(diretorio, recursive: true);
        }
    }

    [Fact]
    public async Task Coletar_usa_identificacao_e_timeout_configurados()
    {
        var manipulador = new ManipuladorFake(requisicao =>
        {
            Assert.Equal("/games", requisicao.RequestUri!.PathAndQuery);

            return Json(JogosDaFonte);
        });

        var diretorio = CriarDiretorio();

        try
        {
            using var cliente = new HttpClient(manipulador);

            await new ColetorSnapshot(cliente).ColetarAsync(
                new OpcoesColeta
                {
                    BaseUrl = BaseUrl,
                    TagsCandidatas = [],
                    TimeoutSegundos = 7
                },
                diretorio);

            Assert.Equal(TimeSpan.FromSeconds(7), cliente.Timeout);
            Assert.Equal(new Uri(BaseUrl), cliente.BaseAddress);
            Assert.Contains(
                "GameMatch.Seed",
                cliente.DefaultRequestHeaders.UserAgent.ToString());
        }
        finally
        {
            Directory.Delete(diretorio, recursive: true);
        }
    }

    private const string JogosDaFonte = """
        [
          {
            "id": 10,
            "title": "Aurora",
            "thumbnail": "https://cdn.exemplo/10.jpg",
            "short_description": "Batalhas espaciais.",
            "game_url": "https://www.freetogame.com/g/10",
            "genre": "Strategy",
            "platform": "PC (Windows)",
            "publisher": "Estúdio",
            "developer": "Estúdio",
            "release_date": "2024-01-01",
            "freetogame_profile_url": "https://www.freetogame.com/aurora"
          },
          {
            "id": 11,
            "title": "Blue Racer",
            "thumbnail": "https://cdn.exemplo/11.jpg",
            "short_description": "Corrida no navegador.",
            "game_url": "https://www.freetogame.com/g/11",
            "genre": "Racing",
            "platform": "Web Browser",
            "publisher": "Estúdio",
            "developer": "Estúdio",
            "release_date": "2023-05-10",
            "freetogame_profile_url": "https://www.freetogame.com/blue-racer"
          }
        ]
        """;

    private static OpcoesColeta Opcoes(string[] tags) => new()
    {
        BaseUrl = BaseUrl,
        TagsCandidatas = tags
    };

    private static HttpResponseMessage Json(string json) => new(HttpStatusCode.OK)
    {
        Content = new StringContent(json, Encoding.UTF8, "application/json")
    };

    private static string CriarDiretorio()
    {
        var caminho = Path.Combine(
            Path.GetTempPath(),
            $"gamematch-coleta-{Guid.NewGuid():N}");

        Directory.CreateDirectory(caminho);
        return caminho;
    }

    private sealed class ManipuladorFake(
        Func<HttpRequestMessage, HttpResponseMessage> responder)
        : HttpMessageHandler
    {
        public List<string> Caminhos { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Caminhos.Add(request.RequestUri!.PathAndQuery);

            return Task.FromResult(responder(request));
        }
    }
}
