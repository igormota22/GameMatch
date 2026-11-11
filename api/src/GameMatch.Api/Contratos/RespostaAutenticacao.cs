namespace GameMatch.Api.Contratos;

public sealed record UsuarioResposta(string Email, DateTimeOffset CriadoEm);

public sealed record RespostaAutenticacao(
    string Token,
    DateTimeOffset ExpiraEm,
    UsuarioResposta Usuario);
