# ADR-001: Contrato de integração entre Sync e consumidores de dados SIGDN-RH

## Status

Accepted

## Contexto

O componente Sync é responsável por obter, normalizar e persistir dados do SAP SIGDN-RH. Existem dois consumidores (PIIP e A2DIP) que precisam de aceder a esses dados no seu próprio formato, sem conhecer detalhes do SAP ou da infraestrutura do Sync.
Os modelos ZhrS\* viviam na Infrastructure do Sync, o que impedia a sua utilização como contrato estável entre componentes.

## Decisão

### 1. Promoção dos modelos ZhrS\* para Application

Os modelos `ZhrSBaseModel`, `ZhrSBaseModelOutput` e todos os tipos concretos (`ZhrSAptidao`, `ZhrSPessoais`, etc.) são promovidos de `Infrastructure` para `Application/Models`, tornando-os independentes de detalhes de persistência.

### 2. Contrato de consumo definido no Sync

O Sync define o contrato que os consumidores implementam.

`ZhrOutput` — vista agregada por `Ni` de todos os dados SAP persistidos:

```csharp
public class ZhrOutput
{
    public required string Ni { get; set; }
    public required string ExternalId { get; set; }
    public ZhrSAptidao[]? Aptidao { get; set; }
    public ZhrSPessoais[]? Pessoais { get; set; }
    // restantes colecções ZhrS*
}
```

`IZhrOutputProvider` — ponto de acesso único aos dados normalizados do SIGDN-RH:

```csharp
public interface IZhrOutputProvider
{
    Task<IReadOnlyList> GetOutputsByNiAsync(IReadOnlyList pessoaSyncRefs, CancellationToken ct);
}
```

Os consumidores obtêm os dados invocando `IZhrOutputProvider`, nunca acedendo directamente à infraestrutura do Sync. A implementação é resolvida via DI em runtime.

Cada consumidor é responsável por definir o seu próprio fluxo de transformação e persistência, composto por três peças internas ao consumidor:

- **Transformer** — transforma `ZhrOutput` para o modelo de destino
- **Persister** — persiste o modelo de destino no schema do consumidor
- **Serviço de orquestração** — coordena o fluxo completo

### 3. Fronteiras entre assemblies

| Assembly              | Responsabilidade                                                                 |
| --------------------- | -------------------------------------------------------------------------------- |
| `Sync.Application`    | Define `ZhrOutput`, `IZhrOutputProvider` e `PessoaSyncRef`                       |
| `Sync.Infrastructure` | Implementa `IZhrOutputProvider` — referencia `Sync.Application`                  |
| `ConsumidorX`         | Implementa transformer, persister e orquestrador — referencia `Sync.Application` |
| Host (por definir)    | Regista `IZhrOutputProvider` via DI                                              |

O Sync não conhece os consumidores. Os consumidores não conhecem o SAP nem a infraestrutura do Sync. O único ponto de acoplamento é o contrato (`ZhrOutput` + `IZhrOutputProvider`).

## Consequências

- Modelos `ZhrS*` estáveis como contrato partilhado interno
- Consumidores isolados da infraestrutura do Sync — dependem apenas de `Sync.Application`
- `Infrastructure` passa a depender de `Application`
- Suporta push diário por delta e pull on-demand sobre o mesmo contrato
- Novos consumidores não implicam alterações no Sync

## Alternativas consideradas

- **`IZhrSyncAdapter<TTarget>` genérico** — rejeitado porque a responsabilidade de transformação e persistência é do consumidor, não do Sync. O provider é um ponto de acesso a dados, não um adaptador de transformação.
- **Contrato definido pelo consumidor** — rejeitado porque o Sync é o produtor dos dados e deve controlar o modelo que expõe.

## Notas de evolução futura

### Substituição da camada de integração SAP

A arquitectura adoptada isola completamente o SAP SIGDN-RH atrás do Sync.
O `ZhrOutput` e o `IZhrOutputProvider` são o único ponto de contacto entre o Sync e os consumidores, sendo que nenhum consumidor tem conhecimento do sistema de origem.

No cenário de migração do SIGDN-RH para SAP S/4HANA (ou outro sistema externo),
o impacto fica contido:

| Componente                      | Impacto                                                              |
| ------------------------------- | -------------------------------------------------------------------- |
| `Sync.Infrastructure`           | Substituída — nova camada de integração com o sistema destino        |
| `Sync.Application/Models`       | Potencialmente revisto se o modelo de dados mudar significativamente |
| `ZhrOutput`                     | Revisto apenas se os dados expostos mudarem                          |
| `IZhrOutputProvider`            | Sem impacto — o contrato não conhece o sistema de origem             |
| Implementações dos consumidores | Impacto proporcional às alterações no `ZhrOutput`                    |
| Modelos de destino              | Sem impacto — são independentes do sistema de origem                 |

A substituição do sistema externo é uma decisão interna ao Sync. Os consumidores só são afectados se o contrato (`ZhrOutput`) mudar e, mesmo nesse caso, a alteração é localizada ao transformer de cada consumidor, não à sua lógica de negócio.

Esta propriedade foi um factor de decisão consciente na adopção desta arquitectura.
