# Design

## Context

Ver `proposal.md` — Why. O perfil atual guarda preferências em
`PerfisPreferencias (Tipo, Valor)` e o caso de uso só valida obrigatórios e
tamanho. O catálogo já expõe `GET /catalogo/filtros` e a consulta de
características distintas em `ICatalogoRepositorio`. A interface de `/perfil`
usa três `textarea` (gêneros, plataformas, estilos) e um quarto para
experiências; a home repete a exibição. O contrato `GET/PUT /perfil` usa 422
para validação.

## Goals / Non-Goals

**Goals:**

- Reaproveitar o vocabulário já calculado pelo catálogo, sem criar uma segunda
  fonte de opções.
- Garantir no servidor a mesma regra que a interface oferece, inclusive para
  chamadas diretas à API.
- Manter dados legados consultáveis e removíveis sem permitir recriação.
- Reduzir o perfil a três fatores (gêneros, plataformas, estilos), alinhado ao
  DT-07/DT-08.

**Non-Goals:**

- Calcular recomendações ou alterar o snapshot/sincronização do catálogo.
- Migrar dados de perfis existentes antecipadamente; a limpeza de experiências
  acontece no próximo salvamento.
- Introduzir busca ou agrupamento nas listas de opções.

## Decisions

### Validação no `ServicoPerfil` reutilizando `ObterFiltrosAsync`

O caso de uso passa a receber `ICatalogoRepositorio` e, depois das validações
de obrigatórios e tamanho, carrega as listas distintas do catálogo e compara
cada valor. Alternativa descartada: consulta dedicada por `(Tipo, Valor)` no
repositório — o catálogo do MVP é pequeno (centenas de jogos), e a consulta de
filtros já existe, é indexada por `(Tipo, Valor)` e é coberta por testes.

- A comparação ignora caixa (`OrdinalIgnoreCase`); o valor persistido é o do
  catálogo. Isso evita divergência de grafia entre perfil e jogo sem exigir que
  a interface seja a única barreira.
- Erros são acumulados por campo (`generos`, `plataformas`, `estilos`) no mesmo
  dicionário devolvido por `ResultadoSalvarPerfil`, mantendo o 422
  ProblemDetails atual.
- Se o catálogo estiver vazio, todo valor novo é rejeitado por ausência de
  correspondência.

### Legado reconhecido pelo perfil atual, não pelo catálogo

Antes de validar, o caso de uso lê o perfil atual do usuário. Um valor fora do
catálogo é aceito somente se já existir naquele perfil, com o mesmo tipo
(comparação sem caixa). O valor legado é preservado como está; se o catálogo
tiver uma grafia equivalente, prevalece a do catálogo. Alternativa descartada:
aceitar qualquer valor já existente globalmente — permitiria copiar valores
órfãos de outro usuário.

Consequência: remover o valor e salvar o exclui; reenviá-lo depois falha com
422. Isso implementa RB-16/CA-22 sem uma tabela de histórico.

### Experiências saem do contrato, mas o enum permanece

`PerfilDto` e `PerfilSalvarDto` deixam de ter `Experiencias`; o JSON enviado
com esse campo é ignorado. `TipoPreferencia.Experiencia` continua no enum para
que perfis antigos possam ser lidos por `PerfilGamer` sem erro, mas `Mapear`
não o expõe e o salvamento não o recria: como `SubstituirAsync` troca todas as
preferências, o próximo `PUT` remove as experiências armazenadas. Não há
migração de banco.

### Interface usa listas de caixas de seleção com valores controlados

Cada campo vira um `FormControl<string[]>` renderizado como grupo de
`checkbox` acessível (fieldset/legend), preenchido a partir de
`CatalogoService.obterFiltros()`. A seleção é alterada por um método que
adiciona/remove o valor do array e marca o controle como tocado. Alternativas
descartadas: `<select multiple>` (pouco usável em telas pequenas e no
teclado/mobile) e manter texto livre validado no envio (não atende RB-16/CA-21).

- As opções são carregadas ao abrir a edição, em paralelo com a leitura do
  perfil; falha ou lista vazia não impede a leitura, mas exibe aviso, botão de
  nova tentativa e desabilita “Salvar perfil”.
- Valores salvos que não estão no catálogo são acrescentados ao grupo em uma
  seção “Fora do catálogo (legado)”, marcados e desmarcáveis. O envio inclui
  somente os marcados.
- Os rótulos continuam “Gêneros favoritos”, “Plataformas” e “Estilos de
  gameplay”; o último recebe as tags do catálogo. O campo e a exibição de
  experiências são removidos de `/perfil` e da home.
- O helper `dividirLinhas` e o validador `aoMenosUmValor` deixam de ser
  necessários no formulário; a obrigatoriedade passa a ser `Validators.required`
  sobre o array (ou validador equivalente que rejeite lista vazia).

### Testes

- Unitários: `ServicoPerfil` com repositório de catálogo fake, cobrindo valor
  válido, valor novo fora do catálogo, legado aceito, legado removido e
  catálogo vazio.
- Integração: semear características no banco antes de salvar perfis (mesmo
  padrão de `CatalogoTests`) para que `RPG`, `PC (Windows)`, `turn-based` etc.
  existam; validar 422 para valor desconhecido e ausência de `experiencias` no
  JSON.
- Web: specs de `perfil` (opções renderizadas, legado, catálogo indisponível,
  envio do contrato sem experiências) e `inicio` (três campos).

## Risks / Trade-offs

- [Atualização do catálogo remove valores salvos] → legado continua legível e
  removível; novos usos são bloqueados, conforme RB-16/CA-22.
- [API nova quebra cliente antigo que lê `experiencias`] → único cliente é a
  web deste repositório; publicar API e web juntas e registrar o **BREAKING**
  no PR.
- [Salvar remove experiências de forma irreversível] → comportamento
  intencional (RF-05/RF-13..15); documentado no PR e coberto por teste.
- [Catálogo vazio ou indisponível impede editar] → leitura permanece; a edição
  mostra aviso e nova tentativa, sem gravar dados inválidos.
- [Duas leituras por salvamento (perfil + filtros)] → volume pequeno e operação
  de escrita pouco frequente; sem cache no MVP.

## Migration Plan

1. Publicar API e web no mesmo release (sem migration EF).
2. Verificar `GET /catalogo/filtros` populado antes de liberar a edição.
3. Rollback: reverter imagens; dados de perfil permanecem, exceto experiências
   já removidas por um salvamento durante a nova versão, que não são
   restauradas.
