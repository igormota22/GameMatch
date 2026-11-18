# Tasks

## 1. Motor de compatibilidade (Domain) — TC-03/TC-04

- [x] 1.1 Criar `api/src/GameMatch.Domain/Recomendacoes/` com os tipos de
      preferências/características de entrada e `CalculadoraCompatibilidade`
      puro (pesos 40/20/25 + 15 no Modo Descoberta, renormalização e
      arredondamento), verificando com `dotnet build api/GameMatch.sln`.
- [x] 1.2 Escrever `api/tests/GameMatch.UnitTests/Recomendacoes/CalculadoraCompatibilidadeTests.cs`
      cobrindo pontuação 0–100, fatores ausentes renormalizados, jogo sem
      correspondência com nota 0, fator de experiência só quando informado e
      soma das contribuições ≈ pontuação (±1), verificando com
      `dotnet test api/GameMatch.sln --filter CalculadoraCompatibilidade`.

## 2. Caso de uso, repositório e configuração

- [x] 2.1 Criar `Application/Recomendacoes/` com `IRecomendacaoRepositorio`
      (preferências + catálogo com características em lote), as classes de
      opção `OpcoesRecomendacoes` (`Recomendacoes:Top`, padrão 12) e
      `OpcoesDescoberta` (`Descoberta:Opcoes`) lidas de `IConfiguration`,
      verificando com `dotnet build api/GameMatch.sln`.
- [x] 2.2 Implementar `Infrastructure/Persistencia/Repositorios/RecomendacaoRepositorio.cs`
      com `AsNoTracking` e leituras em lote, registrando os serviços em
      `Api/Program.cs` e as seções de configuração em `appsettings.json`
      (cinco opções de DT-09), verificando com
      `dotnet test api/GameMatch.sln --filter RecomendacaoRepositorio`.
- [x] 2.3 Escrever `ServicoRecomendacoes` (validação de perfil incompleto →
      `perfil-incompleto`, chave de `experiencia` desconhecida → 404,
      ordenação determinística e limite do topo) com
      `api/tests/GameMatch.UnitTests/Recomendacoes/ServicoRecomendacoesTests.cs`
      usando fakes privados, verificando com
      `dotnet test api/GameMatch.sln --filter ServicoRecomendacoes`.

## 3. API — endpoints e integração (TC-03..TC-06)

- [ ] 3.1 Criar `Api/Controllers/RecomendacaoController.cs` com
      `GET /recomendacoes?experiencia=` e `GET /recomendacoes/opcoes`
      (`[Authorize]`, `Problem` com `traceId` e códigos 401/404/409),
      verificando com `dotnet build api/GameMatch.sln`.
- [ ] 3.2 Escrever `api/tests/GameMatch.IntegrationTests/RecomendacaoTests.cs`
      no padrão de `PerfilTests` (conta real + seed de catálogo) cobrindo 401,
      409 `perfil-incompleto`, 404 para opção inválida, topo-12 ordenado,
      0≤pont≤100 e presença dos fatores na resposta, verificando com
      `dotnet test api/GameMatch.sln --filter RecomendacaoTests`.

## 4. Web — rota, listagem e Modo Descoberta (CA-11..CA-15)

- [ ] 4.1 Criar `core/servicos/recomendacoes.service.ts` e
      `features/recomendacoes/` com rota lazy em `app.routes.ts` protegida por
      `autenticadoGuard` e item "Recomendações" no `layout/cabecalho/cabecalho.html`,
      verificando com `cd web; npm test -- --watch=false` (rota e cabeçalho
      cobertos pelos specs existentes).
- [ ] 4.2 Implementar a tela com cards de pontuação e fatores explicados,
      estados de carregando/erro, CTA para `/perfil` diante de
      `perfil-incompleto` e seletor de experiência alimentado por
      `/recomendacoes/opcoes`, coberto em
      `features/recomendacoes/recomendacoes.spec.ts` (CA-11..CA-15, inclusão do
      fator de experiência e redirecionamento de visitante), verificando com
      `cd web; npm test -- --watch=false`.

## 5. Verificação e documentação

- [ ] 5.1 Rodar `dotnet test api/GameMatch.sln` e `cd web; npm test -- --watch=false`
      no estado final da branch e confirmar as suítes verdes (TC-03..TC-06).
- [ ] 5.2 Revisar manualmente `/recomendacoes` com API + web locais: perfil
      completo lista e explica, perfil incompleto exibe CTA, Modo Descoberta
      muda a lista com fator de experiência; registrar o resultado no PR.
- [ ] 5.3 Atualizar `README.md` (F3 concluída) e o roadmap `docs/tdd.md` §8,
      registrando em `docs/tdd.md` a medição de Q-02 se ela for feita; verificar
      com `git diff --stat`.

## Workflow follow-up

- Abrir PR da feature branch para a `main` com
  `.github/pull_request_template.md`, relacionando RF/CA/DT e testes.
- Depois do merge, arquivar a change com `/opsx-archive` para sincronizar a
  spec `recomendacoes`.
