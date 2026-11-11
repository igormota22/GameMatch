using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace GameMatch.IntegrationTests;

public sealed class ContasTests : IClassFixture<FabricaApiComBanco>
{
    private const string SenhaValida = "Senha123";

    private readonly FabricaApiComBanco _fabrica;

    public ContasTests(FabricaApiComBanco fabrica)
    {
        _fabrica = fabrica;
    }

    [Fact]
    public async Task Cadastro_valido_retorna_token_e_conta_acessivel()
    {
        using var cliente = _fabrica.CreateClient();
        var email = GerarEmail();

        var token = await CriarContaAsync(cliente, email);

        cliente.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        using var resposta = await cliente.GetAsync("/conta");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);

        var conta = await resposta.Content.ReadFromJsonAsync<UsuarioResposta>();
        Assert.NotNull(conta);
        Assert.Equal(email, conta!.Email);
    }

    [Fact]
    public async Task Cadastro_com_senha_fora_das_regras_retorna_400()
    {
        using var cliente = _fabrica.CreateClient();

        using var resposta = await cliente.PostAsJsonAsync(
            "/contas",
            new { email = GerarEmail(), senha = "abc" });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal("application/problem+json", resposta.Content.Headers.ContentType?.MediaType);

        var corpo = await resposta.Content.ReadAsStringAsync();
        Assert.Contains("pelo menos 8", corpo);
    }

    [Fact]
    public async Task Cadastro_com_email_duplicado_retorna_409()
    {
        using var cliente = _fabrica.CreateClient();
        var email = GerarEmail();

        await CriarContaAsync(cliente, email);

        using var resposta = await cliente.PostAsJsonAsync(
            "/contas",
            new { email, senha = SenhaValida });

        Assert.Equal(HttpStatusCode.Conflict, resposta.StatusCode);
        Assert.Equal("application/problem+json", resposta.Content.Headers.ContentType?.MediaType);

        var corpo = await resposta.Content.ReadAsStringAsync();
        Assert.Contains("Já existe uma conta com este e-mail", corpo);
    }

    [Fact]
    public async Task Login_valido_retorna_token()
    {
        using var cliente = _fabrica.CreateClient();
        var email = GerarEmail();

        await CriarContaAsync(cliente, email);

        using var resposta = await cliente.PostAsJsonAsync(
            "/contas/login",
            new { email, senha = SenhaValida });

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);

        var autenticacao = await resposta.Content.ReadFromJsonAsync<RespostaAutenticacao>();
        Assert.NotNull(autenticacao);
        Assert.False(string.IsNullOrWhiteSpace(autenticacao!.Token));
        Assert.Equal(email, autenticacao.Usuario.Email);
    }

    [Fact]
    public async Task Login_com_credenciais_invalidas_retorna_401_generico()
    {
        using var cliente = _fabrica.CreateClient();
        var email = GerarEmail();

        await CriarContaAsync(cliente, email);

        using var resposta = await cliente.PostAsJsonAsync(
            "/contas/login",
            new { email, senha = "OutraSenha123" });

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);

        var corpo = await resposta.Content.ReadAsStringAsync();
        Assert.Contains("Credenciais inválidas", corpo);
        Assert.DoesNotContain("senha atual", corpo, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Conta_sem_token_retorna_401()
    {
        using var cliente = _fabrica.CreateClient();

        using var resposta = await cliente.GetAsync("/conta");

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task Alteracao_de_senha_com_senha_atual_incorreta_retorna_400()
    {
        using var cliente = _fabrica.CreateClient();
        var email = GerarEmail();
        var token = await CriarContaAsync(cliente, email);

        cliente.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        using var resposta = await cliente.PutAsJsonAsync(
            "/conta/senha",
            new { senhaAtual = "Errada123", novaSenha = "NovaSenha123" });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);

        var corpo = await resposta.Content.ReadAsStringAsync();
        Assert.Contains("senha atual", corpo, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Alteracao_de_senha_valida_permite_login_com_nova_senha()
    {
        using var cliente = _fabrica.CreateClient();
        var email = GerarEmail();
        var token = await CriarContaAsync(cliente, email);

        cliente.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        using var alteracao = await cliente.PutAsJsonAsync(
            "/conta/senha",
            new { senhaAtual = SenhaValida, novaSenha = "NovaSenha123" });

        Assert.Equal(HttpStatusCode.NoContent, alteracao.StatusCode);

        cliente.DefaultRequestHeaders.Authorization = null;

        using var login = await cliente.PostAsJsonAsync(
            "/contas/login",
            new { email, senha = "NovaSenha123" });

        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
    }

    private static string GerarEmail() => $"conta.{Guid.NewGuid():N}@teste.com";

    private static async Task<string> CriarContaAsync(
        HttpClient cliente,
        string email,
        string senha = SenhaValida)
    {
        using var resposta = await cliente.PostAsJsonAsync("/contas", new { email, senha });

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);

        var autenticacao = await resposta.Content.ReadFromJsonAsync<RespostaAutenticacao>();
        Assert.NotNull(autenticacao);
        Assert.False(string.IsNullOrWhiteSpace(autenticacao!.Token));

        return autenticacao.Token;
    }

    private sealed record RespostaAutenticacao(
        string Token,
        DateTimeOffset ExpiraEm,
        UsuarioResposta Usuario);

    private sealed record UsuarioResposta(string Email, DateTimeOffset CriadoEm);
}
