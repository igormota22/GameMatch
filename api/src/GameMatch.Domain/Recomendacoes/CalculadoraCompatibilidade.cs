namespace GameMatch.Domain.Recomendacoes;

/// <summary>
/// Motor puro de compatibilidade entre as preferências de um jogador e as
/// características de um jogo (DT-17). Os pesos fixos são os de DT-07:
/// gêneros 40%, plataformas 20%, estilos 25% e experiência 15% no Modo
/// Descoberta. Um grupo sem itens no perfil é ignorado e os pesos são
/// renormalizados entre os fatores definidos. Não há I/O: a classe só recebe
/// e devolve valores.
/// </summary>
public static class CalculadoraCompatibilidade
{
    public const decimal PesoGeneros = 0.40m;
    public const decimal PesoPlataformas = 0.20m;
    public const decimal PesoEstilos = 0.25m;
    public const decimal PesoExperiencia = 0.15m;

    /// <summary>
    /// Calcula a pontuação de um jogo. <paramref name="alvoExperiencia"/> é o
    /// mapeamento da opção do Modo Descoberta escolhida na consulta e, quando
    /// presente, entra como quarto fator; quando ausente, vale o fluxo padrão.
    /// </summary>
    public static ResultadoCalculo Calcular(
        PreferenciasCalculo preferencias,
        CaracteristicasJogo jogo,
        IReadOnlyList<string>? alvoExperiencia = null)
    {
        var definidos = new List<FatorDefinido>();

        Adicionar(
            definidos,
            "generos",
            "Gêneros",
            PesoGeneros,
            preferencias.Generos,
            jogo.Generos);

        Adicionar(
            definidos,
            "plataformas",
            "Plataformas",
            PesoPlataformas,
            preferencias.Plataformas,
            jogo.Plataformas);

        Adicionar(
            definidos,
            "estilos",
            "Estilos de gameplay",
            PesoEstilos,
            preferencias.Estilos,
            jogo.Tags);

        if (alvoExperiencia is not null)
        {
            Adicionar(
                definidos,
                "experiencia",
                "Experiência",
                PesoExperiencia,
                alvoExperiencia,
                Distintos(jogo.Generos.Concat(jogo.Tags)));
        }

        if (definidos.Count == 0)
        {
            return new ResultadoCalculo(0, []);
        }

        var somaPesos = definidos.Sum(fator => fator.Peso);

        if (somaPesos == 0)
        {
            return new ResultadoCalculo(0, []);
        }

        var contribuicoes = new int[definidos.Count];
        var fatores = new FatorCalculo[definidos.Count];

        for (var indice = 0; indice < definidos.Count; indice++)
        {
            var definido = definidos[indice];
            var itensDoPerfil = new HashSet<string>(
                definido.Itens,
                StringComparer.OrdinalIgnoreCase);

            var correspondencias = definido.AlvoJogo
                .Where(valor => itensDoPerfil.Contains(valor))
                .ToArray();

            var fator = itensDoPerfil.Count == 0
                ? 0m
                : (decimal)correspondencias.Length / itensDoPerfil.Count;

            // Peso renormalizado entre os fatores definidos (DT-07).
            var peso = definido.Peso / somaPesos;

            contribuicoes[indice] = (int)Math.Round(
                peso * fator * 100m,
                MidpointRounding.AwayFromZero);

            fatores[indice] = new FatorCalculo(
                definido.Chave,
                definido.Rotulo,
                contribuicoes[indice],
                correspondencias);
        }

        // A pontuação é a soma das contribuições arredondadas: inteira, entre
        // 0 e 100 e sempre igual à explicação exibida ao usuário.
        return new ResultadoCalculo(contribuicoes.Sum(), fatores);
    }

    private static void Adicionar(
        ICollection<FatorDefinido> definidos,
        string chave,
        string rotulo,
        decimal peso,
        IReadOnlyList<string> itens,
        IReadOnlyList<string> alvoJogo)
    {
        // Grupo sem itens no perfil: fator indefinido, ignorado na soma.
        if (itens.Count == 0)
        {
            return;
        }

        definidos.Add(new FatorDefinido(chave, rotulo, peso, itens, alvoJogo));
    }

    private static string[] Distintos(IEnumerable<string> valores)
    {
        var distintos = new List<string>();
        var vistos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var valor in valores)
        {
            if (!string.IsNullOrWhiteSpace(valor) && vistos.Add(valor))
            {
                distintos.Add(valor);
            }
        }

        return distintos.ToArray();
    }

    private sealed record FatorDefinido(
        string Chave,
        string Rotulo,
        decimal Peso,
        IReadOnlyList<string> Itens,
        IReadOnlyList<string> AlvoJogo);
}
