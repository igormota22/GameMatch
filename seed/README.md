# Seed do catálogo

Este diretório guarda o snapshot local do catálogo da FreeToGame, usado para que
a demonstração funcione sem depender de chamadas à API externa.

## Estado atual

O snapshot foi coletado da fonte oficial e está versionado:

| Arquivo | Conteúdo |
|---|---|
| `jogos.json` | 416 jogos retornados por `https://www.freetogame.com/api/games` |
| `tags.json` | mapa de 30 tags para os identificadores externos dos jogos |
| `metadados.json` | data da coleta, contagens, origem e tags ignoradas |

As tags `third-person` e `zombies` não entram no mapa porque a fonte responde
`500` para elas; a ocorrência fica registrada em `tagsIgnoradas`.

## Esquemas

### `jogos.json`

Array com os objetos exatamente como retornados pela API (snake_case), um por
jogo:

```json
[
  {
    "id": 540,
    "title": "Overwatch",
    "thumbnail": "https://www.freetogame.com/g/540/thumbnail.jpg",
    "short_description": "Descrição curta do jogo.",
    "game_url": "https://www.freetogame.com/open/overwatch",
    "genre": "Shooter",
    "platform": "PC (Windows)",
    "publisher": "Blizzard Entertainment",
    "developer": "Blizzard Entertainment",
    "release_date": "2016-05-24",
    "freetogame_profile_url": "https://www.freetogame.com/overwatch"
  }
]
```

O `id` é o identificador externo e é único. `genre` e `platform` vêm como
strings únicas; `publisher` e `release_date` ficam apenas no snapshot e não são
sincronizados.

### `tags.json`

Mapa de tag para os identificadores externos dos jogos que a possuem:

```json
{
  "tags": {
    "action": [1, 2, 3],
    "moba": [2, 7]
  }
}
```

As tags são reconstruídas com uma chamada por tag candidata
(`https://www.freetogame.com/api/filter?tag=<tag>`), porque o payload de jogo não
traz as tags. A lista de tags candidatas fica em
`api/src/GameMatch.Seed/Coleta/OpcoesColeta.cs` e pode ser ajustada em
`Coleta:TagsCandidatas` na configuração do CLI.

### `metadados.json`

```json
{
  "coletadoEm": "2026-11-16T04:44:51.5484177+00:00",
  "totalJogos": 416,
  "totalTags": 30,
  "tagsIgnoradas": ["third-person", "zombies"],
  "origem": "https://www.freetogame.com/api/",
  "versaoFormato": 1
}
```

A sincronização grava `coletadoEm` em `SincronizadoEm` e recusa snapshots com
`versaoFormato` diferente de `1`, contagens divergentes dos arquivos ou
identificador duplicado.

## Procedimento de atualização

Na raiz do repositório, com acesso à internet:

```powershell
dotnet run --project api/src/GameMatch.Seed -- coletar
```

A coleta grava os três arquivos de forma atômica (temporário + troca) e só
substitui o snapshot anterior se a fonte responder. Depois, com o PostgreSQL no
ar e a string de conexão configurada:

```powershell
$env:ConnectionStrings__GameMatch = "Host=localhost;Port=5432;Database=gamematch;Username=gamematch;Password=gamematch-dev"
dotnet run --project api/src/GameMatch.Seed -- sync
```

O `sync` valida o snapshot inteiro antes de abrir a transação, faz upsert por
`id` externo e substitui as características de cada jogo. Jogos que saem da
fonte não são removidos pelo `sync`; a limpeza é manual e está descrita em
[`docs/ambiente.md`](../docs/ambiente.md).

## Regras

- Os arquivos são somente leitura para a aplicação: o banco é populado a partir
  deles, não o contrário. A API e a interface web nunca escrevem aqui.
- Toda alteração no snapshot deve registrar a data de coleta em
  `metadados.json`.
- Nenhum dado sensível ou credencial entra neste diretório.
- A origem dos dados é a FreeToGame (https://www.freetogame.com); a atribuição
  é exibida no rodapé da aplicação web.
