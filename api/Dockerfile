
# Etapa 1: compilação
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY . .

RUN dotnet restore "src/GameMatch.Api/GameMatch.Api.csproj"

RUN dotnet publish "src/GameMatch.Api/GameMatch.Api.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

# Etapa 2: execução
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=10000

EXPOSE 10000

ENTRYPOINT ["dotnet", "GameMatch.Api.dll"]
