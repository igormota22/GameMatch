using Microsoft.AspNetCore.Identity;

namespace GameMatch.Api.Autenticacao;

/// <summary>
/// Traduz os códigos de erro do Identity para mensagens em português usadas no
/// ProblemDetails devolvido pela API.
/// </summary>
public static class MensagensIdentidade
{
    private static readonly Dictionary<string, string> Traducoes = new(StringComparer.Ordinal)
    {
        ["PasswordTooShort"] = "A senha deve ter pelo menos 8 caracteres.",
        ["PasswordRequiresDigit"] = "A senha deve conter ao menos um dígito.",
        ["PasswordRequiresUpper"] = "A senha deve conter ao menos uma letra maiúscula.",
        ["PasswordRequiresLower"] = "A senha deve conter ao menos uma letra minúscula.",
        ["PasswordRequiresNonAlphanumeric"] =
            "A senha deve conter ao menos um caractere especial.",
        ["PasswordRequiresUniqueChars"] =
            "A senha deve conter caracteres únicos suficientes.",
        ["PasswordMismatch"] = "A senha atual informada está incorreta.",
        ["InvalidEmail"] = "Informe um e-mail válido.",
        ["InvalidUserName"] = "O e-mail informado não é válido como nome de usuário.",
        ["DuplicateEmail"] = "Já existe uma conta com este e-mail.",
        ["DuplicateUserName"] = "Já existe uma conta com este e-mail."
    };

    public static IReadOnlyList<string> Traduzir(IEnumerable<IdentityError> erros)
    {
        var mensagens = erros
            .Select(erro => Traducoes.TryGetValue(erro.Code, out var mensagem)
                ? mensagem
                : "Não foi possível concluir a operação. Verifique os dados informados.")
            .Distinct(StringComparer.Ordinal)
            .ToList();

        return mensagens.Count > 0
            ? mensagens
            : ["Não foi possível concluir a operação. Verifique os dados informados."];
    }

    public static bool ContemErroDeDuplicidade(IEnumerable<IdentityError> erros) =>
        erros.Any(erro => erro.Code is "DuplicateEmail" or "DuplicateUserName");
}
