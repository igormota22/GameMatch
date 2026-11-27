# Proposal

## Why

O frontend do GameMatch já está publicado no Cloudflare Pages e encontra a API por uma regra de redirect no painel apontando para uma URL temporária do Cloudflare Tunnel (`trycloudflare.com`), que muda a cada inicialização. Isso exige atualização manual de URL, configuração manual de variáveis antes de cada execução e abertura de vários terminais — e a URL permanente da API não existe hoje. A change resolve isso com uma inicialização única por F5 no VS Code, mantendo o endereço do Pages como único endereço público oficial.

## What Changes

- **Inicialização automatizada via F5**: `launch.json` + `tasks.json` no VS Code executam um script de infraestrutura que (1) verifica os serviços necessários (Docker/PostgreSQL `gamematch-db-1`, `cloudflared`, variáveis do ambiente), (2) inicia a API ASP.NET Core no perfil `Production-Tunnel` (Neon), (3) inicia um Cloudflare Tunnel apontando para a API local, (4) atualiza automaticamente o destino da API no mecanismo de encaminhamento estável e (5) espera o `/health` responder antes de sinalizar "pronto", com logs claros no painel de saída.
- **URL pública permanente (novo)**: um Cloudflare Worker gratuito em `*.workers.dev` passa a ser o endereço estável da API; a regra de redirect do Pages é reconfigurada **uma única vez** para esse Worker e nunca mais precisa de alteração. A cada F5, apenas a origem interna do Worker (URL do túnel rápido atual) é atualizada via API do Cloudflare.
- **Encerramento organizado**: `postDebugTask` encerra os processos iniciados para a execução (cloudflared e estado associado), com arquivo de estado/PIDs; F5 repetido é idempotente (sem processos duplicados nem conflito de portas).
- **Separação de ambientes preservada**: novo arquivo git-ignored `.env.tunnel.example`/`.env.tunnel` com as credenciais do modo público (Neon, chave JWT, origem CORS do Pages, token da API Cloudflare); o modo de desenvolvimento local continua com `.env`/User Secrets + PostgreSQL Docker `gamematch-db-1` + proxy `web/proxy.conf.json` apontando para `http://localhost:5080`.
- **Provisionamento de uma única vez** (fora do fluxo do F5): script `scripts/configurar-cloudflare.ps1` cria/verifica o Worker e a chave de origem e imprime a URL estável; checklist de uma vez inclui criar o API token do Cloudflare e apontar a redirect rule do Pages para o Worker.
- **Sem mudanças de negócio**: nenhuma funcionalidade, regra ou contrato da API é alterado.

Não há dependências pagas: apenas serviços gratuitos do Cloudflare (Workers free tier, túnel rápido, Pages já existente).

## Capabilities

### New Capabilities

- `disponibilizacao-api`: ciclo de vida da disponibilização da API — endereço público estável, encaminhamento da API atrás do túnel, verificação de saúde, separação de configurações entre ambiente público e local e inicialização/encerramento orquestrados pelo VS Code.

### Modified Capabilities

(nenhuma — capacidades existentes: `catalogo`, `favoritos-avaliacoes`, `identidade-visual`, `perfil-gamer`, `recomendacoes`; requisitos dessas capacidades não mudam)

## Impact

**Arquivos novos (repositório):**
- `.vscode/launch.json` e `.vscode/tasks.json` (já permitidos no `.gitIgnore`: `!.vscode/launch.json`, `!.vscode/tasks.json`)
- `scripts/iniciar-execucao.ps1`, `scripts/parar-execucao.ps1`, `scripts/configurar-cloudflare.ps1`
- `.env.tunnel.example` (placeholders; `.env.tunnel` já é coberto pelo ignore `.env.*`)

**Arquivos modificados:**
- `web/proxy.conf.json` — alvo de volta para `http://localhost:5080` (hoje aponta para um `trycloudflare.com` temporário), preservando o dev local
- `.env.example` — novas chaves de exemplo do modo público (somente placeholders)
- `docs/ambiente.md` — seção de execução por F5 e provisionamento de uma única vez

**Fora do repositório (configuração única, documentada no plano):**
- Worker Cloudflare novo (`*.workers.dev`) + API token do Cloudflare (guardado localmente, nunca versionado)
- Redirect rule do Cloudflare Pages reconfigurada uma única vez para a URL do Worker

**Rastreabilidade com os documentos globais (não recriados aqui):**
- `docs/prd.md` — RNF-01 (segredos fora de arquivos versionados), RNF-05 (comportamento da interface com a API indisponível), RNF-09 (`/health` como verificação)
- `docs/tdd.md` — DT-02 (PostgreSQL em Docker no ambiente local), DT-04 (dev server Angular e proxy), DT-16 (verificação local, sem CI); o registro da nova decisão de infraestrutura será **atualizado** em `docs/tdd.md` na implementação, conforme `operations.apply`
