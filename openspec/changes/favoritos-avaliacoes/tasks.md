# Tasks

## 1. Domínio e persistência (F4)

- [ ] 1.1 Criar as entidades `Favorito` e `Avaliacao` no `GameMatch.Domain`
      (PK composta `(UsuarioId, JogoId)`, `CriadoEm`/`AtualizadaEm`, nota 1–5)
      e unit tests de invariantes INV-04/INV-05 — verificação: `dotnet test`
      do `GameMatch.UnitTests` verde com os novos testes.
- [ ] 1.2 Adicionar as configurações EF (`Configuracoes/`) com PK composta e
      CHECK de nota 1–5, registrar no `GameMatchDbContext` e gerar a migration
      `FavoritosAvaliacoes` — verificação: `dotnet ef migrations add` gera a
      migration e `dotnet ef database update` aplica limpo no banco do compose.
- [ ] 1.3 Criar índices `Favoritos (UsuarioId)` e `Avaliacoes (UsuarioId)`
      (§4.3 do TDD) e unit tests das regras de favorito/avaliação citadas na
      §7.1 — verificação: migration contém os índices e `dotnet test` verde.

## 2. API de favoritos

- [ ] 2.1 Criar `Application/Favoritos/` (`IFavoritoRepositorio`,
      `ServicoFavoritos`, DTOs de página e de ids) com as regras de
      idempotência (201/200) e remoção (204/404) — verificação: unit tests do
      serviço cobrem criação, repetição, remoção e favorito inexistente.
- [ ] 2.2 Criar `FavoritoController` com `GET /favoritos`,
      `POST /favoritos/{jogoId}`, `DELETE /favoritos/{jogoId}` e
      `GET /favoritos/ids` (D1), `[Authorize]` e `UsuarioId` do token —
      verificação: testes de integração respondem 201/200/204/401/404 conforme
      §5.1 e o contrato de ids `{ "ids": [] }`.
- [ ] 2.3 Escrever os testes de integração de INV-05/INV-07 (POST paralelo
      sem duplicata; usuário A não acessa favoritos de B) e de listagem vazia/
      paginada — verificação: `dotnet test` do `GameMatch.IntegrationTests`
      verde com os novos cenários.

## 3. API de avaliações

- [ ] 3.1 Criar `Application/Avaliacoes/` (`IAvaliacaoRepositorio`,
      `ServicoAvaliacoes`, DTOs) com upsert (cria ou substitui), remoção e
      consulta — verificação: unit tests cobrem primeira avaliação,
      substituição, remoção e nota fora da faixa.
- [ ] 3.2 Criar `AvaliacaoController` com `GET/PUT/DELETE /avaliacoes/{jogoId}`
      (422 para nota fora de 1–5, 404 para jogo/avaliação inexistente, 401 sem
      sessão) — verificação: testes de integração de 422/404/401/200/204
      conforme §5.1.
- [ ] 3.3 Escrever o teste de concorrência da §7.2 (dois `PUT /avaliacoes`
      paralelos → um único registro) — verificação: teste de integração verde
      com chamadas paralelas.

## 4. Exclusão nas recomendações (DT-12, INV-06)

- [ ] 4.1 Estender `IRecomendacaoRepositorio`/`RecomendacaoRepositorio` para
      devolver os pares `(UsuarioId, JogoId)` favoritados/avaliados do usuário
      e filtrar no `ServicoRecomendacoes` antes do cálculo/top-12 (D2) —
      verificação: unit tests do serviço excluem marcados e mantêm o topo 12
      com catálogo parcialmente marcado.
- [ ] 4.2 Escrever o teste de integração TC-07 de exclusão: favoritado e
      avaliado não aparecem em `GET /recomendacoes`, remoção devolve o jogo —
      verificação: `dotnet test` verde cobrindo os cenários do delta de
      `recomendacoes`.

## 5. Interface web

- [ ] 5.1 Criar `core/servicos/favoritos.service.ts` (signal de ids via
      `GET /favoritos/ids`, favoritar/remover otimistas) e
      `avaliacoes.service.ts`, com unit tests dos estados de erro —
      verificação: `npm test` verde com os specs novos.
- [ ] 5.2 Adicionar o botão de favoritar nos cards de `features/catalogo` e
      `features/recomendacoes` refletindo o estado do serviço — verificação:
      specs de componente cobrem favoritar, remover e falha da API (estado
      revertido + mensagem).
- [ ] 5.3 Criar `features/favoritos/` com rota `/favoritos` (guard
      autenticado): lista paginada, remoção, estrelas 1–5 para avaliar,
      alterar e remover avaliação, estados vazio e de erro — verificação:
      `npm test` verde (specs dos estados) e verificação manual de CA-16/17 no
      navegador.
- [ ] 5.4 Registrar a rota em `app.routes.ts`, item no `cabecalho.html` e
      entrada na home se aplicável — verificação: `npm test` (spec de rotas)
      verde e navegação manual até `/favoritos` a partir do cabeçalho.

## 6. Verificação final e documentação

- [ ] 6.1 Rodar as suítes completas (`dotnet test` e `npm test`) e validar o
      TC-07 (unit + integração) e os cenários dos deltas — verificação:
      suítes verdes sem teste pendente.
- [ ] 6.2 Atualizar `docs/tdd.md` §5.1 com `GET /favoritos/ids` e marcar F4
      como concluída no roadmap §8 (se TC-07 verde), e o `README.md` se
      mencionar fases — verificação: diff revisado, IDs DT-12/INV-06
      referenciados.
- [ ] 6.3 Revisar acessibilidade básica dos fluxos novos (teclado e labels,
      RNF-07) e percorrer as jornadas J-6/J-7 manualmente — verificação:
      checklist de CA-16/CA-17 anotado no PR.

## Workflow follow-up

- Revisar e aprovar as especificações da change antes de implementar
  (regra do `openspec/config.yaml`).
- Abrir PR da feature branch para a `main` com o template de
  `.github/pull_request_template.md`, relacionando RF-16..18, CA-16/17,
  DT-12 e TC-07.
- Arquivar a change (`openspec archive`) após a integração do PR.
