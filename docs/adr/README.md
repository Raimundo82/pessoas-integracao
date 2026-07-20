# Architecture Decision Records

| ID                                                          | Título                                                          | Estado   | Data       |
| :---------------------------------------------------------- | :-------------------------------------------------------------- | :------- | :--------- |
| [ADR-001](./ADR-001-sync-consumers-integration-contract.md) | Contrato de integração entre SYNC e consumidores (PIIP e A2DIP) | Accepted | 2026-07-08 |
| [ADR-002](./ADR-002-sync-orchestration-strategy.md)         | Estratégia de orquestração da ingestão de dados do SIGDN-RH     | Accepted | 2026-07-20 |

## Mapeamento de Componentes

| Alias     | Assembly / Componente          |
| :-------- | :----------------------------- |
| **SYNC**  | `Pessoas.Integracao.Sync`      |
| **PIIP**  | `Pessoas.Integracao.Core`      |
| **A2DIP** | `Pessoas.Integracao.Analitica` |
