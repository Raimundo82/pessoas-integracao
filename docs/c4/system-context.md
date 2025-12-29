# C4 System Context

```plantuml
@startuml system-context-pessoas-integracao
!include https://raw.githubusercontent.com/plantuml-stdlib/C4-PlantUML/master/C4_Context.puml

LAYOUT_WITH_LEGEND()

title System Context - Plataforma de Integração de Pessoas

Enterprise_Boundary(marinha, "Marinha") {
    Person(admin, "Administrador", "Operação e monitorização")
    System(pessoas_integracao, "Plataforma de Integração de Pessoas", "Centraliza, normaliza e distribui dados de pessoas")
    System(consumidores, "Sistemas Consumidores", "Sistemas internos dependentes de dados de pessoas")
}
Enterprise_Boundary(sgmdn, "SGMDN") {
    System_Ext(sigdn_rh, "SIGDN-RH", "Sistema fonte de dados de pessoas")
}
Rel_L(admin, pessoas_integracao,"Configura, monitoriza e executa operações")
Rel_R(consumidores, pessoas_integracao, "Consomem dados de pessoas integrados")
Rel(pessoas_integracao, sigdn_rh, "Obtém dados de pessoas (consulta/extração)")
@enduml
```
