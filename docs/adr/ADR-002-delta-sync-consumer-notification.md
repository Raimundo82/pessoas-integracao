# ADR-002: Orquestração do fluxo de sincronização e notificação de consumidores

## Status

Accepted

## Contexto

O Sync é responsável por processar deltas do SIGDN-RH, obter e persistir os dados actualizados e garantir que os consumidores têm acesso aos dados mais recentes.
Foram consideradas três abordagens para orquestrar este fluxo:

- **Opção A** — Sync publica eventos, consumidores subscrevem
- **Opção B** — Sync invoca uma interface que os consumidores implementam via DI
- **Opção C** - Sync invoca/depende de serviços/componentes dos consumidores
- **Opção D** — orquestração externa ao Sync

## Decisão

Adoptar a **Opção B** — o Sync orquestra o fluxo completo através de uma interface `IZhrDeltasConsumer` que os consumidores implementam e registam via DI.

```csharp
// Sync.Application/Contracts/IZhrDeltaConsumer.cs
public interface IZhrDeltaConsumer
{
    Task OnDeltasProcessedAsync(IReadOnlyList<PessoaSyncRef> refs, CancellationToken ct);
}
```

O fluxo de execução é o seguinte:

1. Cron job despoleta o processamento de deltas no Sync
2. Sync identifica os `PessoaSyncRef` a actualizar
3. Sync obtém e persiste os dados do SIGDN-RH
4. Sync notifica os consumidores registados via `IZhrDeltasConsumer`
5. Cada consumidor obtém os dados através de `IZhrOutputProvider`, transforma e persiste no seu schema de destino

## Consequências

- Sync controla o fluxo completo sem conhecer as implementações concretas dos consumidores
- Na eventualidade de ser necessário adicionar novos consumidores, este registam `IZhrDeltasConsumer` via DI sem propagação de alterações no Sync
- O contrato de notificação (`IZhrDeltasConsumer`) vive em `Sync.Application/Contracts` junto dos restantes contratos

## Alternativas rejeitadas

**Opção A — eventos/mensageria**
Mais desacoplada mas requer infraestrutura adicional (message broker) que não se justifica para o número actual de consumidores e contexto do projecto.

**Opção C — Sync invoca/depende de serviços/componentes dos consumidores**
O Sync passaria a conhecer e depender directamente dos assemblies dos consumidores, invertendo as dependências e criando um acoplamento bidirecional. Qualquer alteração
interna de um consumidor propagaria para o Sync, violando o princípio de que o Sync é o upstream e os consumidores são quem depende dele.

**Opção D — orquestração externa ao Sync**
O Sync perderia a capacidade de gerir a sua própria responsabilidade que passa por garantir que os dados estão actualizados e que os consumidores são notificados. Introduz dependência de um componente
externo para cumprir uma responsabilidade interna do Sync.
