using GameMatch.Infrastructure.Persistencia;
using GameMatch.Seed.Coleta;
using GameMatch.Seed.Snapshot;
using GameMatch.Seed.Sincronizacao;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GameMatch.Seed;

/// <summary>
/// Composição do executável de operação do catálogo. Os comandos são manuais e
/// não fazem parte do processo da API.
/// </summary>
internal static class AplicacaoCli
{
    private const string Uso = """
        Uso: GameMatch.Seed <comando> [opções]

        Comandos:
          coletar   Gera jogos.json, tags.json e metadados.json a partir da FreeToGame
          sync      Sincroniza o banco a partir do snapshot local

        Opções:
          --saida <diretório>        Diretório de saída da coleta (padrão: seed)
          --snapshot <diretório>     Diretório do snapshot para o sync (padrão: seed)
          --string-conexao <valor>   String de conexão do PostgreSQL para o sync
        """;

    public static async Task<int> ExecutarAsync(string[] args)
    {
        if (args.Length == 0)
        {
            await Console.Error.WriteLineAsync(Uso);
            return 2;
        }

        var comando = args[0];

        if (comando is "--help" or "-h" or "ajuda")
        {
            Console.WriteLine(Uso);
            return 0;
        }

        if (comando is not ("coletar" or "sync"))
        {
            await Console.Error.WriteLineAsync($"Comando desconhecido: {comando}");
            await Console.Error.WriteLineAsync(Uso);
            return 2;
        }

        var construtor = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
        {
            Args = args,
            ContentRootPath = AppContext.BaseDirectory
        });

        construtor.Services.AddHttpClient<ColetorSnapshot>();

        using var host = construtor.Build();
        var configuracao = host.Services.GetRequiredService<IConfiguration>();

        try
        {
            return comando == "coletar"
                ? await ColetarAsync(host.Services, configuracao)
                : await SincronizarAsync(configuracao);
        }
        catch (FormatoSnapshotInvalidoException excecao)
        {
            await Console.Error.WriteLineAsync($"erro: {excecao.Message}");
            return 1;
        }
        catch (Exception excecao)
        {
            await Console.Error.WriteLineAsync($"erro: {excecao.Message}");
            return 1;
        }
    }

    private static async Task<int> ColetarAsync(
        IServiceProvider servicos,
        IConfiguration configuracao)
    {
        var opcoes = OpcoesColeta.DeConfiguracao(configuracao);
        var diretorio = configuracao["saida"] ?? DiretorioPadraoSnapshot();
        var coletor = servicos.GetRequiredService<ColetorSnapshot>();

        var resumo = await coletor.ColetarAsync(
            opcoes,
            diretorio,
            mensagem => Console.WriteLine($"aviso: {mensagem}"));

        Console.WriteLine($"Snapshot gravado em {Path.GetFullPath(diretorio)}");
        Console.WriteLine($"  jogos: {resumo.TotalJogos}");
        Console.WriteLine($"  tags: {resumo.TotalTags}");
        Console.WriteLine($"  coletado em: {resumo.ColetadoEm:O}");

        if (resumo.TagsIgnoradas.Count > 0)
        {
            Console.WriteLine($"  tags ignoradas: {string.Join(", ", resumo.TagsIgnoradas)}");
        }

        return 0;
    }

    private static async Task<int> SincronizarAsync(IConfiguration configuracao)
    {
        var diretorio = configuracao["snapshot"] ?? DiretorioPadraoSnapshot();
        var stringConexao = configuracao["string-conexao"]
            ?? configuracao.GetConnectionString("GameMatch");

        if (string.IsNullOrWhiteSpace(stringConexao))
        {
            await Console.Error.WriteLineAsync(
                "erro: a string de conexão não foi configurada. Defina " +
                "ConnectionStrings__GameMatch ou use --string-conexao.");
            return 1;
        }

        var snapshot = new LeitorSnapshot().Ler(diretorio);

        var opcoes = new DbContextOptionsBuilder<GameMatchDbContext>()
            .UseNpgsql(stringConexao)
            .Options;

        await using var contexto = new GameMatchDbContext(opcoes);
        var resumo = await new SincronizadorCatalogo(contexto).SincronizarAsync(snapshot);

        Console.WriteLine($"Catálogo sincronizado a partir de {Path.GetFullPath(diretorio)}");
        Console.WriteLine($"  inseridos: {resumo.Inseridos}");
        Console.WriteLine($"  atualizados: {resumo.Atualizados}");
        Console.WriteLine($"  tags ignoradas: {resumo.ReferenciasIgnoradas}");
        Console.WriteLine($"  coletado em: {snapshot.Metadados.ColetadoEm:O}");

        return 0;
    }

    /// <summary>
    /// Usa o diretório <c>seed/</c> da raiz do repositório; se a raiz não for
    /// encontrada, cai para <c>seed/</c> no diretório atual. A raiz é
    /// identificada pelo diretório <c>.git</c>, porque a solução também existe
    /// em <c>api/</c>.
    /// </summary>
    private static string DiretorioPadraoSnapshot()
    {
        var atual = new DirectoryInfo(AppContext.BaseDirectory);

        while (atual is not null)
        {
            if (Directory.Exists(Path.Combine(atual.FullName, ".git")))
            {
                return Path.Combine(atual.FullName, "seed");
            }

            atual = atual.Parent;
        }

        return Path.Combine(Directory.GetCurrentDirectory(), "seed");
    }
}
