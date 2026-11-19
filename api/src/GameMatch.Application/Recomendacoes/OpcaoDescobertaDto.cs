namespace GameMatch.Application.Recomendacoes;

/// <summary>
/// Opção do Modo Descoberta exposta ao cliente: a chave enviada em
/// `experiencia` e o rótulo exibido no seletor (RF-14).
/// </summary>
public sealed record OpcaoDescobertaDto(string Chave, string Rotulo);
