# Architecture Decision Records

## GEN

| ID                                                              | Título                                                                                         | Estado | Data       |
| :-------------------------------------------------------------- | :--------------------------------------------------------------------------------------------- | :----- | :--------- |
| [001-GEN](./001-GEN-adopt-datetimeoffset-and-utc-invariant.md)  | Adotar `DateTimeOffset` para todas as datas e horas e definir UTC como fuso horário invariante | Aceite | 2026-07-26 |
| [002-GEN](./002-GEN-explicit-midnight-for-daily-granularity.md) | Definir explicitamente `00:00:00` para granularidade diária com `DateTimeOffset`               | Aceite | 2026-07-26 |

## SYNC

| ID                                                                            | Título                                                                      | Estado   | Data       |
| :---------------------------------------------------------------------------- | :-------------------------------------------------------------------------- | :------- | :--------- |
| [001-SYNC](./001-SYNC-consumers-integration-contract.md)                      | Contrato de integração entre SYNC e consumidores de dados SIGDN-RH          | Aceite   | 2026-07-25 |
| [002-SYNC](./002-SYNC-implement-zhr-freshness-checker.md)                     | Componente `IZhrFreshnessChecker` para verificação de dados ZHR atualizados | Aceite   | 2026-07-26 |
| [003-SYNC](./003-SYNC-encapsulation-of-enrichment-logic-for-synchronizers.md) | Encapsulamento da lógica de enriquecimento para sincronizadores             | Aceite   | 2026-07-27 |
| [004-SYNC](./004-SYNC-generic-zhr-ws-client-pattern.md)                       | Cliente Genérico para Serviços Web ZHR                                      | Aceite   | 2026-07-28 |
| [005-SYNC](./005-SYNC-sigdn-response-validation.md)                           | Validação Centralizada de Respostas SAP por `PessoaSyncRef`                 | Proposto | 2026-07-29 |

## A2DIP

| ID                                                                       | Título                                                                                      | Estado | Data       |
| :----------------------------------------------------------------------- | :------------------------------------------------------------------------------------------ | :----- | :--------- |
| [001-A2DIP](./001-A2DIP-migration-zhrwsbasemodel-to-ianalitica-model.md) | Migração de ZhrWsBaseModel para IAnaliticaModel e AnaliticaBaseModel                        | Aceite | 2026-07-24 |
| [002-A2DIP](./002-A2DIP-adopt-riok-mapperly-for-model-mapping.md)        | Adoção do Riok.Mapperly para mapeamento de modelos no assembly Pessoas.Integracao.Analitica | Aceite | 2026-07-28 |

## Mapeamento de Assemblies

| Alias     | Assembly / Componente          |
| :-------- | :----------------------------- |
| **GEN**   | All for general purpose        |
| **SYNC**  | `Pessoas.Integracao.Sync`      |
| **PIIP**  | `Pessoas.Integracao.Core`      |
| **A2DIP** | `Pessoas.Integracao.Analitica` |
