# Use Case - SyncAnaliticaData - Sequence Diagram

```plantuml
@startuml
note across
Este diagrama documenta o fluxo do AnaliticaSyncOrchestrator: recebe um
ZhrOutput do SYNC, distribui o trabalho por N Synchronizers (uma por coleção
- Aptidoes, Pessoais, Familias, ... ~N no total), e cada Synchronizer mapeia
+ persiste a sua fatia de forma independente.

O orquestrador nunca conhece tipos concretos (ZhrSAptidao, ZhrWsAptidaoAptidao,
etc.) - só conhece IAnaliticaSynchronizer. Adicionar uma coleção nova
nunca requer alterar este fluxo.
end note

== Execução - AnaliticaSyncOrchestrator ==

box "Pessoas.Integracao.Analitica.Infrastructure" #LightBlue
    participant "Invoker" as Caller
    participant "AnaliticaSyncOrchestrator" as Orchestrator
    participant "IAnaliticaSynchronizer[]" as Synchronizers


Caller -> Orchestrator : ExecuteAsync(input : ZhrOutput)
activate Orchestrator

    par para cada strategy em strategies
        Orchestrator -> Synchronizers : SyncAsync(input, ct)
    end

deactivate Orchestrator

== Detalhe de uma strategy (representativo - AnaliticaAptidaoSynchronizer) ==

    participant "AnaliticaAptidaoSynchronizer" as Synchronizer
    participant "AptidaoMapper" as Mapper
    participant "IAnaliticaRepository<ZhrWsAptidaoAptidao>" as Repo
end box

activate Orchestrator
Orchestrator -> Synchronizer : SyncAsync(input, ct)
activate Synchronizer

alt input.Aptidoes é null ou vazio
    Synchronizer --> Orchestrator : retorna imediatamente\n(nenhuma chamada a Mapper/Repo)
else input.Aptidoes tem dados
    loop para cada ZhrSAptidao em input.Aptidoes
        Synchronizer -> Mapper : Map(source, input.ExternalId)
        activate Mapper
        Mapper --> Synchronizer : ZhrWsAptidaoAptidao\n(Ni por convenção, Numsap = ExternalId,\nId/UpdatedAt ignorados)
        deactivate Mapper
    end

    Synchronizer -> Repo : ReplaceMatchingByNiAsync(mapped, ct)
    activate Repo
    Repo --> Synchronizer : OK
    deactivate Repo

    Synchronizer --> Orchestrator : concluído
end

deactivate Synchronizer
deactivate Orchestrator

note across
Este mesmo padrão (guard clause -> Mapper -> lista mapeada -> Repository)
repete-se, uma Synchronizer por coleção, para todas as ~N restantes
(PessoalSyncConcreteStrategy, FamiliaSyncConcreteStrategy,
DeficienciaSyncConcreteStrategy, ...).
end note

@enduml
```
