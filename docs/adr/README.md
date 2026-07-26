# Architecture Decision Records

## SYNC

| ID                                                                                                        | Título                                                                                                       | Estado   | Data       |
| :-------------------------------------------------------------------------------------------------------- | :----------------------------------------------------------------------------------------------------------- | :------- | :--------- |
| [001-SYNC-consumers-integration-contract](./001-SYNC-consumers-integration-contract.md)                   | Contrato de integração entre SYNC e consumidores de dados SIGDN-RH                                           | Accepted | 2026-07-25 |
| [002-SYNC-implement-zhr-freshness-checker-service](./002-SYNC-implement-zhr-freshness-checker-service.md) | Implementar o componente `IZhrFreshnessChecker` para verificação de _freshness_ via consulta à base de dados | Proposed | 2026-07-26 |

## Mapeamento de Componentes

| Alias     | Assembly / Componente          |
| :-------- | :----------------------------- |
| **SYNC**  | `Pessoas.Integracao.Sync`      |
| **PIIP**  | `Pessoas.Integracao.Core`      |
| **A2DIP** | `Pessoas.Integracao.Analitica` |
