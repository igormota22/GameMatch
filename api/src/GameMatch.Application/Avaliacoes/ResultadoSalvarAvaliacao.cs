namespace GameMatch.Application.Avaliacoes;

/// <summary>
/// Desfecho do salvamento da avaliação: gravada, nota fora da faixa de 1 a 5
/// (422) ou jogo inexistente no catálogo (404).
/// </summary>
public enum StatusSalvarAvaliacao
{
    Salvo,
    NotaInvalida,
    JogoNaoEncontrado
}

/// <summary>
/// Resultado do caso de uso de avaliação: a nota gravada ou o motivo da
/// rejeição, sem efeito no banco.
/// </summary>
public sealed record ResultadoSalvarAvaliacao(
    StatusSalvarAvaliacao Status,
    AvaliacaoDto? Avaliacao)
{
    public bool Sucesso => Status == StatusSalvarAvaliacao.Salvo;

    public static ResultadoSalvarAvaliacao Ok(int nota) =>
        new(StatusSalvarAvaliacao.Salvo, new AvaliacaoDto(nota));

    public static ResultadoSalvarAvaliacao ForaDaFaixa() =>
        new(StatusSalvarAvaliacao.NotaInvalida, null);

    public static ResultadoSalvarAvaliacao JogoInexistente() =>
        new(StatusSalvarAvaliacao.JogoNaoEncontrado, null);
}
