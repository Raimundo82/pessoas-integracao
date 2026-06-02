# C4 Container Diagram - Analitica View

```plantuml
@startuml

!include https://raw.githubusercontent.com/plantuml-stdlib/C4-PlantUML/master/C4_Container.puml

LAYOUT_WITH_LEGEND()

title Container Diagram - PIIP

Enterprise_Boundary(marinha, "Marinha") {

    Person(admin, "Administrador", "Operação e monitorização")
    Person(analista, "Analista de Informação", "Consulta dados analíticos para processamento")
    System_Boundary(piip, "PIIP") {

        Container(api_admin, "API de Administração", "REST", "Expõe operações administrativas")

        Container(orchestrator, "Orquestrador de Eventos de Domínio", ".NET", "Coordena a execução dos casos de uso e publica eventos de domínio")

        Container(worker, "Worker de Integração", ".NET", "Coordena a extração de dados do SIGDN e prepara snapshots")

        Container(core, "Core das Pessoas", ".NET", "Normaliza informação das pessoas, deteta alterações organizacionais e gere persistência")

        Container(analitica, "Analítica das Pessoas", ".NET", "Processa e persiste dados analíticos derivados dos webservices SIGDN")

        ContainerDb(db_oper, "Base de Dados Operacional", "SQL", "Armazena dados integrados das Pessoas")
        ContainerDb(db_analitica, "Base de Dados Analítica", "SQL", "Armazena dados analíticos derivados do webservices SIGDN")
    }
}

Enterprise_Boundary(sgmdn, "SGMDN") {
    System_Ext(sigdn, "SIGDN-RH", "Sistema fonte dos dados das pessoas")
}

' --- Relations ---
Rel(admin, api_admin, "Executa operações administrativas")
Rel(api_admin, orchestrator, "Invoca GetChanges")
Rel(orchestrator, worker, "Executa fluxo de extração de dados")

Rel(worker, sigdn, "Obtém dados brutos do SIGDN")

Rel(orchestrator, core, "Publica PessoasAtualizadasEvent")
Rel(orchestrator, analitica, "Publica AnaliticaAtualizadaEvent")

Rel(core, db_oper, "Lê/Escreve dados operacionais")
Rel(analitica, db_analitica, "Lê/Escreve dados analíticos")

Rel(analista, db_analitica, "Consulta dados analíticos")

Rel(worker, orchestrator, "Devolve snapshots preparados")

@enduml
```
