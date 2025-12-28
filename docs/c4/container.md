# C4 Container Diagram

```plantuml
@startuml
!include https://raw.githubusercontent.com/plantuml-stdlib/C4-PlantUML/master/C4_Container.puml
' uncomment the following line and comment the first to use locally
' !include C4_Container.puml

'LAYOUT_TOP_DOWN()
'LAYOUT_AS_SKETCH()
LAYOUT_WITH_LEGEND()

title Container Diagram - Plataforma de Integração de Pessoas

Enterprise_Boundary(marinha, "Marinha") {
    System(consumidores, "Sistemas Consumidores", "Sistemas internos dependentes de dados de pessoas")
    System_Boundary(pessoas_integracao, "Plataforma de Integração de Pessoas") {
        Container(core, "Core de Integração de Pessoas", ".NET", "Obtém dados externos, mapeia para o domínio e persiste")
        Container(api, "API de Integração","GraphQL","Interface de acesso para sistemas consumidores")
        ContainerDb(db, "Base de Dados", "SQL", "Armazena dados de pessoas integrados")
    }
}
Enterprise_Boundary(sgmdn, "SGMDN") {
    System_Ext(sigdn_rh, "SIGDN-RH", "Sistema fonte de dados de pessoas")
}

Rel_R(consumidores, api, "Consulta dados de pessoas")
Rel_D(api, core, "Solicita operações sobre dados de pessoas")
Rel_D(core, db, "Lê e escreve dados de pessoas")
Rel_R(core, sigdn_rh, "Obtém dados de pessoas")
@enduml
```
