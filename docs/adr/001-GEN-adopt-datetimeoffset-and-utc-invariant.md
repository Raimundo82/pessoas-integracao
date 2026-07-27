# Adotar `DateTimeOffset` para todas as datas e horas e definir UTC como fuso horário invariante

Status: Proposed

## Contexto e Declaração do Problema

No desenvolvimento de sistemas que lidam com datas e horas, a escolha do tipo de dados adequado é crucial para evitar ambiguidades, erros de fuso horário e inconsistências na persistência e transmissão de dados. Em projetos que integram dados de sistemas externos e gerem informações de pessoal, é essencial garantir que todos os valores de data e hora sejam tratados de forma consistente e inequívoca.

O problema que se pretende resolver é definir de forma clara e uniforme qual o tipo de dados a utilizar para representar datas e horas em todo o projeto, bem como definir o fuso horário invariante a ser utilizado.

## Opções Consideradas

- Utilizar `DateTime` com `Kind` definido como `Utc` ou `Local`
- Utilizar `DateOnly` para datas sem hora e `DateTime` para datas com hora
- Utilizar `DateTimeOffset` para todos os casos de datas e horas

## Resultado da Decisão

**Opção escolhida:** "Utilizar `DateTimeOffset` para todos os casos de datas e horas", porque é a única opção que resolve de forma inequívoca as questões de fuso horário, garante a imutabilidade e a clareza semântica dos valores de data e hora, e evita ambiguidades relacionadas com a conversão entre fusos horários ou a interpretação de `DateTime.Kind`.

O fuso horário deverá ser sempre o mesmo invariante: UTC.

### Consequências

#### Positivas

- A utilização de `DateTimeOffset` garante que a data e hora são sempre acompanhadas do respetivo fuso horário, evitando ambiguidades e erros de conversão.

#### Negativas

- Pode exigir ajustes nos locais onde `DateTime` ou `DateOnly` eram utilizados anteriormente, requerendo refatoração para adotar `DateTimeOffset`.
