using GameMatch.Api.Autenticacao;
using GameMatch.Api.Diagnostico;
using GameMatch.Application.Catalogo;
using GameMatch.Application.Perfil;
using GameMatch.Application.Recomendacoes;
using GameMatch.Infrastructure;
using GameMatch.Infrastructure.Persistencia.Repositorios;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

var stringConexao = builder.Configuration.GetConnectionString("GameMatch");
if (string.IsNullOrWhiteSpace(stringConexao))
{
    throw new InvalidOperationException(
        "A string de conexão 'ConnectionStrings:GameMatch' não foi configurada. " +
        "Defina a variável de ambiente ConnectionStrings__GameMatch ou use " +
        "dotnet user-secrets set \"ConnectionStrings:GameMatch\" \"<string de conexão>\".");
}

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddCors(opcoes =>
{
    var origensPermitidas = builder.Configuration
        .GetSection("Cors:AllowedOrigins")
        .Get<string[]>() ?? [];

    opcoes.AddPolicy("Frontend", politica =>
    {
        if (origensPermitidas.Length > 0)
        {
            politica
                .WithOrigins(origensPermitidas)
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    });
});
builder.Services.AddProblemDetails(opcoes =>
{
    opcoes.CustomizeProblemDetails = contexto =>
    {
        contexto.ProblemDetails.Extensions["traceId"] = contexto.HttpContext.TraceIdentifier;
    };
});
builder.Services.AdicionarInfraestrutura(stringConexao);
builder.Services.AddScoped<IPerfilRepositorio, PerfilRepositorio>();
builder.Services.AddScoped<ServicoPerfil>();
builder.Services.AddScoped<ICatalogoRepositorio, CatalogoRepositorio>();
builder.Services.AddScoped<ServicoCatalogo>();
builder.Services.AddScoped<IRecomendacaoRepositorio, RecomendacaoRepositorio>();
builder.Services.AddSingleton(
    new OpcoesRecomendacoes
    {
        Top = builder.Configuration.GetValue<int?>("Recomendacoes:Top")
            ?? OpcoesRecomendacoes.TopPadrao
    });
builder.Services.AddSingleton(
    new OpcoesDescoberta
    {
        Opcoes = builder.Configuration
            .GetSection("Descoberta:Opcoes")
            .Get<List<OpcaoDescoberta>>() ?? []
    });
builder.Services.AddScoped<ServicoRecomendacoes>();
builder.Services.AdicionarAutenticacaoJwt(builder.Configuration);
builder.Services.AddSingleton<GeradorTokenJwt>();
builder.Services.AddHealthChecks()
    .AddCheck<VerificacaoSaudePostgres>("postgresql");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(opcoes =>
    {
        opcoes.SwaggerEndpoint("/openapi/v1.json", "GameMatch API v1");
        opcoes.RoutePrefix = "swagger";
    });
}

app.Use(async (contexto, proximo) =>
{
    const string cabecalhoCorrelacao = "X-Correlation-ID";

    var correlacao = contexto.Request.Headers[cabecalhoCorrelacao].FirstOrDefault();
    if (string.IsNullOrWhiteSpace(correlacao))
    {
        correlacao = contexto.TraceIdentifier;
    }

    contexto.TraceIdentifier = correlacao;
    contexto.Response.Headers[cabecalhoCorrelacao] = correlacao;

    using (app.Logger.BeginScope("CorrelationId: {CorrelationId}", correlacao))
    {
        await proximo();
    }
});

app.UseExceptionHandler();
app.UseStatusCodePages(async contexto =>
{
    var servicoProblemDetails = contexto.HttpContext.RequestServices
        .GetRequiredService<IProblemDetailsService>();

    await servicoProblemDetails.TryWriteAsync(new ProblemDetailsContext
    {
        HttpContext = contexto.HttpContext,
        ProblemDetails = new ProblemDetails
        {
            Status = contexto.HttpContext.Response.StatusCode,
            Title = "Erro na requisição",
            Detail = $"A requisição retornou o status {contexto.HttpContext.Response.StatusCode}."
        }
    });
});
app.UseHttpsRedirection();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program;
