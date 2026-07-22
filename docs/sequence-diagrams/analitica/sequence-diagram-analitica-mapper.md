# Use Case - SyncAnaliticaData - Sequence Diagram

```plantuml
@startuml
note across
Este diagrama documenta o fluxo do SyncAnaliticaCollections: recebe um
ZhrOutput do SYNC, distribui o trabalho por N Strategies (uma por coleção
- Aptidoes, Pessoais, Familias, ... ~N no total), e cada Strategy mapeia
+ persiste a sua fatia de forma independente.
 
O orquestrador nunca conhece tipos concretos (ZhrSAptidao, ZhrWsAptidaoAptidao,
etc.) - só conhece ICollectionSyncStrategy. Adicionar uma coleção nova
nunca requer alterar este fluxo.
end note
 
== Execução - SyncAnaliticaCollections ==
 
box "Pessoas.Integracao.Analitica.Infrastructure" #LightBlue
    participant "Invoker" as Caller
    participant "SyncAnaliticaCollections" as Orchestrator
    participant "ICollectionSyncStrategy[]" as Strategies

 
Caller -> Orchestrator : ExecuteAsync(input : ZhrOutput)
activate Orchestrator
 
    loop para cada strategy em strategies (sequencial)
        Orchestrator -> Strategies : SyncAsync(input, ct)
    end
 
deactivate Orchestrator
 
== Detalhe de uma strategy (representativo - AptidaoSyncConcreteStrategy) ==
 
    participant "AptidaoSyncConcreteStrategy" as Strategy
    participant "AptidaoMapper" as Mapper
    participant "IAnaliticaRepository<ZhrWsAptidaoAptidao>" as Repo
end box
 
Orchestrator -> Strategy : SyncAsync(input, ct)
activate Strategy
 
alt input.Aptidoes é null ou vazio
    Strategy --> Orchestrator : retorna imediatamente\n(nenhuma chamada a Mapper/Repo)
else input.Aptidoes tem dados
    loop para cada ZhrSAptidao em input.Aptidoes
        Strategy -> Mapper : Map(source, input.ExternalId)
        activate Mapper
        Mapper --> Strategy : ZhrWsAptidaoAptidao\n(Ni por convenção, Numsap = ExternalId,\nId/UpdatedAt ignorados)
        deactivate Mapper
    end
 
    Strategy -> Repo : ReplaceMatchingByNiAsync(mapped, ct)
    activate Repo
    Repo --> Strategy : OK
    deactivate Repo
 
    Strategy --> Orchestrator : concluído
end
 
deactivate Strategy
 
note across
Este mesmo padrão (guard clause -> Mapper -> lista mapeada -> Repository)
repete-se, uma Strategy por coleção, para todas as ~N restantes
(PessoalSyncConcreteStrategy, FamiliaSyncConcreteStrategy,
DeficienciaSyncConcreteStrategy, ...).
end note
 
@enduml
```
