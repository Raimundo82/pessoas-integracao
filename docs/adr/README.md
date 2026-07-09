# Architecture Decision Records

| ID                                                                | Título                                                                  | Estado   | Data       |
| :---------------------------------------------------------------- | :---------------------------------------------------------------------- | :------- | :--------- |
| [ADR-001](./ADR-001-sync-consumers-integration-contract.md)       | Contrato de integração entre SYNC e consumidores                        | Accepted | 2026-07-08 |
| [ADR-002](./ADR-002-delta-sync-consumer-notification-contract.md) | Contrato de notificação de deltas processados entre SYNC e consumidores | Accepted | 2026-07-10 |

## Mapeamento de Componentes

| Alias     | Assembly / Componente          |
| :-------- | :----------------------------- |
| **SYNC**  | `Pessoas.Integracao.Sync`      |
| **PIIP**  | `Pessoas.Integracao.Core`      |
| **A2DIP** | `Pessoas.Integracao.Analitica` |
