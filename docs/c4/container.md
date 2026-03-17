# C4 Container Diagram

```plantuml
@startuml
!include https://raw.githubusercontent.com/plantuml-stdlib/C4-PlantUML/master/C4_Container.puml

LAYOUT_WITH_LEGEND()

title Container Diagram - Plataforma de Integração de Informação das Pessoas (PIIP)
Enterprise_Boundary(marinha, "Marinha") {
    Person(admin, "Administrador", "Operação e monitorização")
    System(consumidores, "Aplicações e Sistemas Consumidores", "Aplicações e Sistemas internos dependentes dos dados das pessoas")
    System_Boundary(pessoas_integracao, "PIIP") {
        Container(api_admin, "API de Administração", "REST", "Monitoriza e Executa operações administrativas sobre a plataforma")
        Container(api_consulta, "API de Consulta", "GraphQL", "Disponibiliza os dados das pessoas a aplicações/sistemas consumidores")
        Container(core, "Core das Pessoas", ".NET", "Normaliza dados das pessoas e gere a persistência")
        Container(worker, "Worker de Integração", ".NET", "Extrai e processa os dados das pessoas")
        ContainerDb(db, "Base de Dados", "SQL", "Armazena os dados integrados das pessoas")
    }
}

Enterprise_Boundary(sgmdn, "SGMDN") {
    System_Ext(sigdn_rh, "SIGDN-RH", "Sistema fonte dos dados das pessoas")
}

Rel_D(consumidores, api_consulta, "Consulta os dados das pessoas")
Rel_R(admin, api_admin, "Monitoriza e Executa operações administrativas")
Rel_D(api_consulta, core, "Solicita operações sobre os dados das pessoas")
Rel_D(api_admin, core, "Solicita operações administrativas")
Rel_R(worker, sigdn_rh, "Obtém os dados das pessoas")
Rel_L(worker, core, "Envia os dados das pessoas")
Rel_D(core, db, "Lê e escreve os dados das pessoas")
@enduml
```
