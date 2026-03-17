# C4 Container Diagram

```plantuml
@startuml
!include https://raw.githubusercontent.com/plantuml-stdlib/C4-PlantUML/master/C4_Container.puml

LAYOUT_WITH_LEGEND()

title Container Diagram - Plataforma de Integração de Informação das Pessoas (PIIP)

Enterprise_Boundary(marinha, "Marinha") {

    Person(admin, "Administrador", "Operação e monitorização")

    System(consumidores, "Aplicações e Sistemas Consumidores", "Aplicações e Sistemas internos dependentes da informação das pessoas")

    System(broker, "Plataforma de Integração Assíncrona", "Distribuição assíncrona de informação entre sistemas")

    System_Boundary(pessoas_integracao, "PIIP") {

        Container(api_admin, "API de Administração", "REST", "Monitoriza e executa operações administrativas sobre a plataforma")

        Container(api_consulta, "API de Consulta", "GraphQL", "Disponibiliza informação integrada das pessoas a sistemas consumidores")

        Container(core, "Core das Pessoas", ".NET", "Normaliza informação das pessoas, deteta alterações organizacionais e gere persistência")

        Container(worker, "Worker de Integração", ".NET", "Extrai e prepara informação das pessoas a partir dos sistemas fonte")

        Container(publisher, "Serviço de Publicação de Eventos Organizacionais", ".NET Worker", "Publica informação com impacto organizacional pendente na plataforma de integração assíncrona")

        ContainerDb(db, "Base de Dados Operacional", "SQL", "Armazena a informação integrada das pessoas")

        ContainerDb(outbox, "Outbox de Eventos Organizacionais", "SQL", "Armazena informação organizacional pendente de publicação")
    }
}

Enterprise_Boundary(sgmdn, "SGMDN") {
    System_Ext(sigdn_rh, "SIGDN-RH", "Sistema fonte dos dados das pessoas")
}

Rel_D(consumidores, api_consulta, "Consulta informação das pessoas")

Rel_R(admin, api_admin, "Monitoriza e executa operações administrativas")

Rel_D(api_consulta, core, "Solicita operações sobre a informação das pessoas")

Rel_D(api_admin, core, "Solicita operações administrativas")

Rel_R(worker, sigdn_rh, "Obtém informação das pessoas")

Rel_L(worker, core, "Envia informação preparada")

Rel_D(core, db, "Lê e escreve informação")

Rel_D(core, outbox, "Regista informação organizacional")

Rel_R(publisher, outbox, "Lê informação pendente")

Rel_R(publisher, broker, "Publica informação organizacional")

Rel(consumidores, broker, "Consomem informação organizacional")

@enduml
```
