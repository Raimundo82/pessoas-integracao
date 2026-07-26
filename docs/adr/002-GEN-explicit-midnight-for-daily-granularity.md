# Definir explicitamente `00:00:00` para granularidade diária com `DateTimeOffset`

Status: Proposed

## Contexto e Declaração do Problema

Quando o objetivo é ter granularidade de datas por dia, é necessário definir explicitamente qual o instante do dia que deve ser utilizado, de forma a garantir consistência em todas as operações de filtro, comparação e persistência.

Mesmo utilizando `DateTimeOffset` como tipo para representar datas e horas, é necessário definir qual o instante do dia que deve ser utilizado quando a granularidade é diária.

## Opções Consideradas

- Implicit midnight (relying on default constructors or string parsing)
- Explicitamente definir `00:00:00` com o offset UTC (`+00:00`)

## Resultado da Decisão

Opção escolhida: "Sempre que o objetivo for ter granularidade de datas por dia, o instante do dia deverá ser o primeiro, ou seja, `00:00:00`, devendo ser explicitamente definido. A granularidade para `TimeSpan` deve ser segundo, portanto a meia-noite será `00:00:00`. Por exemplo, ao representar um dia específico como um `DateTimeOffset`, este deve ser criado com a hora, minuto e segundo definidos como `00:00:00` e o respetivo `Offset` de UTC (`+00:00`), de forma explícita e não implícita."

### Consequências

- Positivo, porque garante consistência nas operações de filtro, comparação e persistência de dados por dia.
