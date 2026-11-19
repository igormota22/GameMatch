using GameMatch.Domain.Entidades;

namespace GameMatch.Application.Catalogo;

/// <summary>
/// Projeção de um jogo do catálogo no resumo consumido pelo catálogo, pelos
/// favoritos e pelas recomendações.
/// </summary>
internal static class MapeadorJogoResumo
{
    internal static JogoResumoDto Mapear(Jogo jogo) =>
        new(
            jogo.Id,
            jogo.Titulo,
            jogo.Thumbnail,
            Valores(jogo, TipoCaracteristica.Genero),
            Valores(jogo, TipoCaracteristica.Plataforma),
            jogo.Desenvolvedor);

    private static string[] Valores(Jogo jogo, TipoCaracteristica tipo) =>
        jogo.Caracteristicas
            .Where(caracteristica => caracteristica.Tipo == tipo)
            .Select(caracteristica => caracteristica.Valor)
            .OrderBy(valor => valor, StringComparer.OrdinalIgnoreCase)
            .ToArray();
}
