using Microsoft.Extensions.Configuration;

namespace GameMatch.Seed.Coleta;

/// <summary>
/// Parâmetros da coleta do snapshot. As tags candidatas são curadas porque a
/// API da FreeToGame não expõe a lista de tags.
/// </summary>
public sealed class OpcoesColeta
{
    public const string UrlPadrao = "https://www.freetogame.com/api/";

    public const int TimeoutPadraoSegundos = 30;

    /// <summary>
    /// Tags conhecidas do catálogo da FreeToGame. Tags que não retornarem jogos
    /// são omitidas e registradas nos metadados.
    /// </summary>
    public static readonly string[] TagsPadrao =
    [
        "3d",
        "action",
        "anime",
        "battle-royale",
        "card",
        "fantasy",
        "fighting",
        "first-person",
        "horror",
        "low-spec",
        "military",
        "mmo",
        "mmofps",
        "mmorpg",
        "moba",
        "open-world",
        "pvp",
        "racing",
        "sandbox",
        "sci-fi",
        "shooter",
        "space",
        "sports",
        "strategy",
        "survival",
        "tank",
        "third-person",
        "top-down",
        "tower-defense",
        "turn-based",
        "voxel",
        "zombies"
    ];

    public string BaseUrl { get; init; } = UrlPadrao;

    public int TimeoutSegundos { get; init; } = TimeoutPadraoSegundos;

    public IReadOnlyList<string> TagsCandidatas { get; init; } = TagsPadrao;

    public static OpcoesColeta DeConfiguracao(IConfiguration configuracao)
    {
        var tags = configuracao
            .GetSection("Coleta:TagsCandidatas")
            .Get<string[]>();

        return new OpcoesColeta
        {
            BaseUrl = configuracao["Coleta:BaseUrl"] ?? UrlPadrao,
            TimeoutSegundos = configuracao.GetValue(
                "Coleta:TimeoutSegundos",
                TimeoutPadraoSegundos),
            TagsCandidatas = tags is { Length: > 0 } ? tags : TagsPadrao
        };
    }
}
