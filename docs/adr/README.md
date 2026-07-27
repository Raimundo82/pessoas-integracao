# Architecture Decision Records

## GEN

| ID                                                              | Título                                                                                         | Estado | Data       |
| :-------------------------------------------------------------- | :--------------------------------------------------------------------------------------------- | :----- | :--------- |
| [001-GEN](./001-GEN-adopt-datetimeoffset-and-utc-invariant.md)  | Adotar `DateTimeOffset` para todas as datas e horas e definir UTC como fuso horário invariante | Aceite | 2026-07-26 |
| [002-GEN](./002-GEN-explicit-midnight-for-daily-granularity.md) | Definir explicitamente `00:00:00` para granularidade diária com `DateTimeOffset`               | Aceite | 2026-07-26 |

## SYNC

| ID                                                       | Título                                                             | Estado | Data       |
| :------------------------------------------------------- | :----------------------------------------------------------------- | :----- | :--------- |
| [001-SYNC](./001-SYNC-consumers-integration-contract.md) | Contrato de integração entre SYNC e consumidores de dados SIGDN-RH | Aceite | 2026-07-25 |

## Mapeamento de Componentes

| Alias     | Assembly / Componente          |
| :-------- | :----------------------------- |
| **GEN**   | All for general purpose        |
| **SYNC**  | `Pessoas.Integracao.Sync`      |
| **PIIP**  | `Pessoas.Integracao.Core`      |
| **A2DIP** | `Pessoas.Integracao.Analitica` |
