# ADR-001: Contrato de integração entre SYNC e consumidores de dados SIGDN-RH

## Status

Accepted

## Contexto

O componente SYNC é responsável por obter, normalizar e persistir dados do SAP SIGDN-RH. Existem dois consumidores (PIIP e A2DIP) que precisam de aceder a esses dados no seu próprio formato, sem conhecer detalhes do SAP ou da infraestrutura do SYNC.
Os modelos ZhrS* viviam na Infrastructure do SYNC, o que impedia a sua utilização como contrato estável entre componentes.

## Decisão

### 1. Promoção dos modelos ZhrS* para Application

Os modelos `ZhrSBaseModel`, `ZhrSBaseModelOutput` e todos os tipos concretos (`ZhrSAptidao`, `ZhrSPessoais`, etc.) são promovidos de `Infrastructure` para `Application/Models`, tornando-os independentes de detalhes de persistência.

### 2. Contrato de consumo definido no SYNC

O SYNC define o contrato que os consumidores implementam.

`ZhrOutputDto` — vista agregada por `Ni` de todos os dados SAP persistidos:

```csharp
public class ZhrOutputDto
{
    public required string Ni { get; set; }
    public required string ExternalId { get; set; }
    public ZhrSAptidao[]? Aptidao { get; set; }
    public ZhrSPessoais[]? Pessoais { get; set; }
    // restantes colecções ZhrS*
}
```

`IZhrSyncAdapter<TTarget>` — interface genérica que cada consumidor implementa com o seu próprio modelo de destino:

```csharp
public interface IZhrSyncAdapter<TTarget>
{
    Task<TTarget> TransformAsync(ZhrOutputDto input, CancellationToken ct);
}
```

### 3. Fronteiras entre assemblies

| Assembly              | Responsabilidade                                     |
| :-------------------- | :--------------------------------------------------- |
| `Sync.Application`    | Define `ZhrOutputDto` e `IZhrSyncAdapter<<TTTarget>` |
| `Sync.Infrastructure` | Persistência — referencia `Sync.Application`         |
| `ConsumidorX`         | Implementa `IZhrSyncAdapter<<TargetTargetModelX>`    |
| `Host` (por definir)  | Regista implementações via DI                        |

O SYNC não conhece os consumidores. Os consumidores não conhecem o SAP nem a infraestrutura do SYNC. O único ponto de acoplamento é o contrato (`ZhrOutputDto` + `IZhrSyncAdapter<<TTTarget>`).

### 4. Registo DI em ponto de entrada aplicacional a definir

```csharp
services.AddScoped<IZhrSyncAdapter<Pessoa>, ZhrA2dipAdapter>();
services.AddScoped<IZhrSyncAdapter<AnaliticaModel>, ZhrAnaliticaAdapter>();
```

## Consequências

- Modelos ZhrS* estáveis como contrato partilhado interno
- Novos consumidores implementam `IZhrSyncAdapter<T>` sem tocar no SYNC
- `Infrastructure` passa a depender de `Application`
- Suporta push diário por delta e pull on-demand sobre o mesmo contrato

## Alternativas consideradas

- **DTOs específicos por consumidor** — rejeitado por duplicação desnecessária dado que os ZhrS* já são suficientemente limpos e estáveis para servir de contrato
- **Contrato definido pelo consumidor** — rejeitado porque o SYNC é o produtor dos dados e deve controlar o modelo que expõe

## Notas de evolução futura

### Substituição da camada de integração SAP

A arquitectura adoptada isola completamente o SAP SIGDN-RH atrás do SYNC. O `ZhrOutputDto` e o `IZhrSyncAdapter<TTarget>` são o único ponto de contacto entre o SYNC e os consumidores — nenhum consumidor tem conhecimento do sistema de origem.

No cenário de migração do SIGDN-RH para SAP S/4HANA (ou outro sistema externo), o impacto fica contido:

| Componente                      | Impacto                                                              |
| :------------------------------ | :------------------------------------------------------------------- |
| `Sync.Infrastructure`           | Substituída — nova camada de integração com o sistema destino        |
| `Sync.Application/Models`       | Potencialmente revisto se o modelo de dados mudar significativamente |
| `ZhrOutputDto`                  | Revisto apenas se os dados expostos mudarem                          |
| `IZhrSyncAdapter<<TTTarget>`    | Sem impacto — o contrato não conhece o sistema de origem             |
| Implementações dos consumidores | Impacto proporcional às alterações no `ZhrOutputDto`                 |
| Modelos de destino (`TTarget`)  | Sem impacto — são independentes do sistema de origem                 |

Em suma: a substituição do sistema externo é uma decisão interna ao SYNC. Os consumidores só são afectados se o contrato (`ZhrOutputDto`) mudar — e mesmo nesse caso, a alteração é localizada à implementação do adaptador de cada consumidor, não à sua lógica de negócio.

Esta propriedade foi um factor de decisão consciente na adopção desta arquitectura.
