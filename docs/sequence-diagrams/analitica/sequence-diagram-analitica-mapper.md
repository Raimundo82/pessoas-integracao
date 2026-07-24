# Use Case - SyncAnaliticaData - Sequence Diagram

```plantuml
@startuml
note across
Este diagrama documenta o fluxo do AnaliticaSyncOrchestrator: recebe um
ZhrOutput do SYNC, distribui o trabalho por N Syncronizers (uma por coleção
- Aptidoes, Pessoais, Familias, ... ~N no total), e cada Syncronizer mapeia
+ persiste a sua fatia de forma independente.
 
O orquestrador nunca conhece tipos concretos (ZhrSAptidao, ZhrWsAptidaoAptidao,
etc.) - só conhece IAnaliticaSynchronizer. Adicionar uma coleção nova
nunca requer alterar este fluxo.
end note
 
== Execução - AnaliticaSyncOrchestrator ==
 
box "Pessoas.Integracao.Analitica.Infrastructure" #LightBlue
    participant "Invoker" as Caller
    participant "AnaliticaSyncOrchestrator" as Orchestrator
    participant "IAnaliticaSynchronizer[]" as Syncronizers

 
Caller -> Orchestrator : ExecuteAsync(input : ZhrOutput)
activate Orchestrator
 
    par para cada strategy em strategies
        Orchestrator -> Syncronizers : SyncAsync(input, ct)
    end
 
deactivate Orchestrator
 
== Detalhe de uma strategy (representativo - AnaliticaAptidaoSyncronizer) ==
 
    participant "AnaliticaAptidaoSyncronizer" as Syncronizer
    participant "AptidaoMapper" as Mapper
    participant "IAnaliticaRepository<ZhrWsAptidaoAptidao>" as Repo
end box

activate Orchestrator
Orchestrator -> Syncronizer : SyncAsync(input, ct)
activate Syncronizer
 
alt input.Aptidoes é null ou vazio
    Syncronizer --> Orchestrator : retorna imediatamente\n(nenhuma chamada a Mapper/Repo)
else input.Aptidoes tem dados
    loop para cada ZhrSAptidao em input.Aptidoes
        Syncronizer -> Mapper : Map(source, input.ExternalId)
        activate Mapper
        Mapper --> Syncronizer : ZhrWsAptidaoAptidao\n(Ni por convenção, Numsap = ExternalId,\nId/UpdatedAt ignorados)
        deactivate Mapper
    end
 
    Syncronizer -> Repo : ReplaceMatchingByNiAsync(mapped, ct)
    activate Repo
    Repo --> Syncronizer : OK
    deactivate Repo
 
    Syncronizer --> Orchestrator : concluído
end
 
deactivate Syncronizer
deactivate Orchestrator
 
note across
Este mesmo padrão (guard clause -> Mapper -> lista mapeada -> Repository)
repete-se, uma Syncronizer por coleção, para todas as ~N restantes
(PessoalSyncConcreteStrategy, FamiliaSyncConcreteStrategy,
DeficienciaSyncConcreteStrategy, ...).
end note
 
@enduml
```
