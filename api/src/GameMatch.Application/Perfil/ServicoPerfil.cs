using GameMatch.Domain.Entidades;
using GameMatch.Domain.Preferencias;

namespace GameMatch.Application.Perfil;

/// <summary>
/// Caso de uso do perfil gamer: leitura sem efeito colateral, validação das
/// preferências obrigatórias e salvamento por substituição.
/// </summary>
public sealed class ServicoPerfil(IPerfilRepositorio repositorio)
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
        var experiencias = NormalizadorPreferencias.NormalizarLista(entrada.Experiencias);

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
        ValidarTamanho("experiencias", experiencias, erros);

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
        Adicionar(perfil, TipoPreferencia.Experiencia, experiencias);

        await repositorio.SubstituirAsync(perfil, cancellationToken);

        return ResultadoSalvarPerfil.Ok(Mapear(perfil));
    }

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
            Filtrar(preferencias, TipoPreferencia.Experiencia),
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
        new([], [], [], [], false, null);
}
