# Use Case - ImportAllPessoas - Sequence Diagram

```plantuml

@startuml

note across
    Nota: Os detalhes da BD e os métodos específicos
    para a infraestrutura SOAP foram omitidos
    para maior clareza e legibilidade do diagrama
end note

actor Administrator as Admin

box "Pessoas.Integracao.Admin" #LightBlue
participant PessoasImportController
end box


box "Pessoas.Integracao.Core" #LightGreen
participant ImportAllPessoas
participant PessoasRepository
end box

box "Pessoas.Integracao.Worker" #LightYellow
participant SigdnRhPessoasProvider
end box

box "SIGDN" #LightGray
participant SIGDN_WS
end box


Admin -> PessoasImportController : POST /api/pessoas/input
activate PessoasImportController

PessoasImportController -> ImportAllPessoas : ExecuteAsync()
activate ImportAllPessoas

ImportAllPessoas -> ImportAllPessoas : GetDistinctImportIdsAsync()
activate ImportAllPessoas

ImportAllPessoas -> SigdnRhPessoasProvider : GetProviderImportIds()
activate SigdnRhPessoasProvider
SigdnRhPessoasProvider -> SIGDN_WS : call SOAP ZhrWsGetPernr endpoint
activate SIGDN_WS
SIGDN_WS --> SigdnRhPessoasProvider : ListaPerNr
deactivate SIGDN_WS
SigdnRhPessoasProvider --> ImportAllPessoas : List<ImportIds>
deactivate SigdnRhPessoasProvider
ImportAllPessoas -> PessoasRepository : GetAllPessoas()
activate PessoasRepository
PessoasRepository --> ImportAllPessoas : List<Pessoa>
deactivate PessoasRepository
ImportAllPessoas -> ImportAllPessoas : GetRepositoryImportIds()
activate ImportAllPessoas
ImportAllPessoas --> ImportAllPessoas : List<ImportIds>
deactivate ImportAllPessoas
ImportAllPessoas -> ImportAllPessoas : UnionImportIds()
activate ImportAllPessoas
ImportAllPessoas --> ImportAllPessoas : List<ImportIds> (deduplicated)
deactivate ImportAllPessoas
deactivate ImportAllPessoas

ImportAllPessoas -> SigdnRhPessoasProvider : GetPessoasByIdentifier(List<ImportIds>)
activate SigdnRhPessoasProvider

loop for each endpoint implementation
    SigdnRhPessoasProvider -> SIGDN_WS : call SOAP endpoint
    activate SIGDN_WS
    SIGDN_WS --> SigdnRhPessoasProvider : response
    deactivate SIGDN_WS
end

SigdnRhPessoasProvider --> ImportAllPessoas : List<Pessoa>
deactivate SigdnRhPessoasProvider

ImportAllPessoas -> PessoasRepository : AddOrUpdateAll(List<Pessoa>)
activate PessoasRepository
PessoasRepository --> ImportAllPessoas : Task<int added, int updated>
deactivate PessoasRepository

ImportAllPessoas -> PessoasRepository : CommitAsync()
activate PessoasRepository
PessoasRepository --> ImportAllPessoas : Task completed
deactivate PessoasRepository

ImportAllPessoas --> PessoasImportController: ImportAllPessoasResult
deactivate ImportAllPessoas

PessoasImportController --> Admin : 202 Accepted (ImportAllPessoasResult)
deactivate PessoasImportController

@enduml
```
