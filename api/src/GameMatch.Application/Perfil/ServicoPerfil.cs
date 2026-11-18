using GameMatch.Application.Catalogo;
using GameMatch.Domain.Entidades;
using GameMatch.Domain.Preferencias;

namespace GameMatch.Application.Perfil;

/// <summary>
/// Caso de uso do perfil gamer: leitura sem efeito colateral, validação das
/// preferências obrigatórias e do vocabulário do catálogo, e salvamento por
/// substituição.
/// </summary>
public sealed class ServicoPerfil(
    IPerfilRepositorio repositorio,
    ICatalogoRepositorio catalogo)
{
    public async Task<PerfilDto> ObterAsync(
        string usuarioId,
        CancellationToken cancellationToken = default)
    {
        var perfil = await repositorio.ObterAsync(usuarioId, cancellationToken);

        return perfil is null ? CriarVazio() : Mapear(perfil);
    }

    public async Task<ResultadoSalvarPerfil> SalvarAsync(
        string usuarioId,
        PerfilSalvarDto entrada,
        CancellationToken cancellationToken = default)
    {
        var generos = NormalizadorPreferencias.NormalizarLista(entrada.Generos);
        var plataformas = NormalizadorPreferencias.NormalizarLista(entrada.Plataformas);
        var estilos = NormalizadorPreferencias.NormalizarLista(entrada.Estilos);

        var erros = new Dictionary<string, string[]>();

        if (generos.Count == 0)
        {
            erros["generos"] = ["Informe ao menos um gênero."];
        }

        if (plataformas.Count == 0)
        {
            erros["plataformas"] = ["Informe ao menos uma plataforma."];
        }

        ValidarTamanho("generos", generos, erros);
        ValidarTamanho("plataformas", plataformas, erros);
        ValidarTamanho("estilos", estilos, erros);

        if (erros.Count > 0)
        {
            return ResultadoSalvarPerfil.Falha(erros);
        }

        var perfilAtual = await repositorio.ObterAsync(usuarioId, cancellationToken);
        var filtros = await catalogo.ObterFiltrosAsync(cancellationToken);

        generos = Canonicalizar(
            generos,
            filtros.Generos,
            ValoresLegados(perfilAtual, TipoPreferencia.Genero),
            "generos",
            erros);
        plataformas = Canonicalizar(
            plataformas,
            filtros.Plataformas,
            ValoresLegados(perfilAtual, TipoPreferencia.Plataforma),
            "plataformas",
            erros);
        estilos = Canonicalizar(
            estilos,
            filtros.Tags,
            ValoresLegados(perfilAtual, TipoPreferencia.Estilo),
            "estilos",
            erros);

        if (erros.Count > 0)
        {
            return ResultadoSalvarPerfil.Falha(erros);
        }

        var perfil = new PerfilGamer
        {
            UsuarioId = usuarioId,
            AtualizadoEm = DateTimeOffset.UtcNow
        };

        Adicionar(perfil, TipoPreferencia.Genero, generos);
        Adicionar(perfil, TipoPreferencia.Plataforma, plataformas);
        Adicionar(perfil, TipoPreferencia.Estilo, estilos);

        await repositorio.SubstituirAsync(perfil, cancellationToken);

        return ResultadoSalvarPerfil.Ok(Mapear(perfil));
    }

    private static IReadOnlyList<string> Canonicalizar(
        IReadOnlyList<string> valores,
        IReadOnlyList<string> doCatalogo,
        IReadOnlyList<string> legados,
        string campo,
        IDictionary<string, string[]> erros)
    {
        if (valores.Count == 0)
        {
            return valores;
        }

        var catalogoPorCaixa = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var valor in doCatalogo)
        {
            catalogoPorCaixa.TryAdd(valor, valor);
        }

        var legadosPorCaixa = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var valor in legados)
        {
            legadosPorCaixa.TryAdd(valor, valor);
        }

        var resultado = new List<string>(valores.Count);
        var invalidos = new List<string>();

        foreach (var valor in valores)
        {
            if (catalogoPorCaixa.TryGetValue(valor, out var canonico) ||
                legadosPorCaixa.TryGetValue(valor, out canonico))
            {
                resultado.Add(canonico);
                continue;
            }

            invalidos.Add(valor);
        }

        if (invalidos.Count > 0)
        {
            erros[campo] =
            [
                $"Escolha valores do catálogo. Fora do catálogo: {string.Join(", ", invalidos)}."
            ];
        }

        return resultado;
    }

    private static IReadOnlyList<string> ValoresLegados(
        PerfilGamer? perfil,
        TipoPreferencia tipo) =>
        perfil?.Preferencias
            .Where(preferencia => preferencia.Tipo == tipo)
            .Select(preferencia => preferencia.Valor)
            .ToArray() ?? [];

    private static void Adicionar(
        PerfilGamer perfil,
        TipoPreferencia tipo,
        IEnumerable<string> valores)
    {
        foreach (var valor in valores)
        {
            perfil.Preferencias.Add(new PreferenciaPerfil
            {
                UsuarioId = perfil.UsuarioId,
                Tipo = tipo,
                Valor = valor
            });
        }
    }

    private static void ValidarTamanho(
        string campo,
        IReadOnlyList<string> valores,
        IDictionary<string, string[]> erros)
    {
        if (valores.Any(valor => valor.Length > NormalizadorPreferencias.TamanhoMaximoValor))
        {
            erros[campo] =
            [
                $"Cada valor deve ter no máximo {NormalizadorPreferencias.TamanhoMaximoValor} caracteres."
            ];
        }
    }

    private static PerfilDto Mapear(PerfilGamer perfil)
    {
        var preferencias = perfil.Preferencias
            .OrderBy(preferencia => preferencia.Valor, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return new PerfilDto(
            Filtrar(preferencias, TipoPreferencia.Genero),
            Filtrar(preferencias, TipoPreferencia.Plataforma),
            Filtrar(preferencias, TipoPreferencia.Estilo),
            perfil.EstaCompleto,
            perfil.AtualizadoEm);
    }

    private static string[] Filtrar(
        IEnumerable<PreferenciaPerfil> preferencias,
        TipoPreferencia tipo) =>
        preferencias
            .Where(preferencia => preferencia.Tipo == tipo)
            .Select(preferencia => preferencia.Valor)
            .ToArray();

    private static PerfilDto CriarVazio() =>
        new([], [], [], false, null);
}
