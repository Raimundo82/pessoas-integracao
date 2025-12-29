# C4 Container Diagram

```plantuml
@startuml
!include https://raw.githubusercontent.com/plantuml-stdlib/C4-PlantUML/master/C4_Container.puml

LAYOUT_WITH_LEGEND()

title Container Diagram - Plataforma de Integração de Pessoas
Enterprise_Boundary(marinha, "Marinha") {
    Person(admin, "Administrador", "Operação e monitorização")
    System(consumidores, "Sistemas Consumidores", "Sistemas internos dependentes de dados de pessoas")
    System_Boundary(pessoas_integracao, "Plataforma de Integração de Pessoas") {
        Container(api_admin, "API de Administração", "REST", "Executa operações administrativas sobre a plataforma")
        Container(api_consulta, "API de Consulta", "GraphQL", "Disponibiliza dados de pessoas a sistemas consumidores")
        Container(core, "Core de Pessoas", ".NET", "Normaliza dados de pessoas e gere a persistência")
        Container(worker, "Worker de Integração", ".NET", "Extrai e orquestra a integração de dados de pessoas")
        ContainerDb(db, "Base de Dados", "SQL", "Armazena dados de pessoas integrados")
    }
}

Enterprise_Boundary(sgmdn, "SGMDN") {
    System_Ext(sigdn_rh, "SIGDN-RH", "Sistema fonte de dados de pessoas")
}

Rel_D(consumidores, api_consulta, "Consulta dados de pessoas")
Rel_R(admin, api_admin, "Executa operações administrativas")
Rel_D(api_consulta, core, "Solicita operações sobre dados de pessoas")
Rel_D(api_admin, core, "Solicita operações administrativas")
Rel_R(worker, sigdn_rh, "Extrai dados de pessoas")
Rel_L(worker, core, "Envia dados de pessoas")
Rel_D(core, db, "Lê e escreve dados de pessoas")
@enduml
```
