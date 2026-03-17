# C4 System Context

```plantuml
@startuml system-context-pessoas-integracao
!include https://raw.githubusercontent.com/plantuml-stdlib/C4-PlantUML/master/C4_Context.puml

LAYOUT_WITH_LEGEND()

title System Context - Plataforma de Integração de Informação das Pessoas (PIIP)

Enterprise_Boundary(marinha, "Marinha") {
    Person(admin, "Administrador", "Operação e monitorização")
    System(pessoas_integracao, "PIIP", "Centraliza, normaliza e disponibiliza os dados das pessoas")
    System(consumidores, "Aplicações e Sistemas Consumidores", "Aplicações e Sistemas internos dependentes dos dados das pessoas")
}
Enterprise_Boundary(sgmdn, "SGMDN") {
    System_Ext(sigdn_rh, "SIGDN-RH", "Sistema fonte dos dados das pessoas")
}
Rel_L(admin, pessoas_integracao,"Configura, monitoriza e executa operações")
Rel_R(consumidores, pessoas_integracao, "Consomem os dados integrados das pessoas")
Rel(pessoas_integracao, sigdn_rh, "Obtém os dados das pessoas (consulta/extração)")
@enduml
```
