using System.ComponentModel.DataAnnotations;

namespace GameMatch.Api.Contratos;

public sealed record CadastroRequisicao(
    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    string Email,
    [Required(ErrorMessage = "A senha é obrigatória.")]
    string Senha);

public sealed record LoginRequisicao(
    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    string Email,
    [Required(ErrorMessage = "A senha é obrigatória.")]
    string Senha);

public sealed record AlteracaoSenhaRequisicao(
    [Required(ErrorMessage = "A senha atual é obrigatória.")]
    string SenhaAtual,
    [Required(ErrorMessage = "A nova senha é obrigatória.")]
    string NovaSenha);
