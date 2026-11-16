namespace GameMatch.Domain.Preferencias;

/// <summary>
/// Regras de normalização compartilhadas pela API e pelos testes. Os valores
/// são aparados, têm espaços internos colapsados e são deduplicados sem
/// diferenciar maiúsculas de minúsculas, preservando a primeira grafia.
/// </summary>
public static class NormalizadorPreferencias
{
    public const int TamanhoMaximoValor = 100;

    public static string Normalizar(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return string.Empty;
        }

        return string.Join(
            ' ',
            valor.Split(
                (char[]?)null,
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
    }

    public static IReadOnlyList<string> NormalizarLista(IEnumerable<string>? valores)
    {
        if (valores is null)
        {
            return Array.Empty<string>();
        }

        var normalizados = new List<string>();
        var vistos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var valor in valores)
        {
            var normalizado = Normalizar(valor);

            if (normalizado.Length > 0 && vistos.Add(normalizado))
            {
                normalizados.Add(normalizado);
            }
        }

        return normalizados;
    }
}
