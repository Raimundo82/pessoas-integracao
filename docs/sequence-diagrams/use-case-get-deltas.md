# Use Case - GetDeltas - Sequence Diagram

```plantuml

@startuml

note across
    Nota: Os detalhes da BD e os métodos específicos
    para a infraestrutura SOAP foram omitidos
    para maior clareza e legibilidade do diagrama
end note

actor Administrator as Admin

box "Pessoas.Integracao.Admin" #LightBlue
participant PessoasDeltasController
end box


box "Pessoas.Integracao.Core" #LightGreen
participant DeltasPessoas
participant PessoasRepository
end box

box "Pessoas.Integracao.Worker" #LightYellow
participant SigdnRhPessoasProvider
end box

box "SIGDN" #LightGray
participant SIGDN_WS
end box


Admin -> PessoasDeltasController : POST /api/pessoas/deltas \n{startTimestamp, endTimestamp}
activate PessoasDeltasController

PessoasDeltasController -> DeltasPessoas : ExecuteAsync()
activate DeltasPessoas

DeltasPessoas -> DeltasPessoas : timePeriod(startTimestamp, endTimestamp)
activate DeltasPessoas
deactivate DeltasPessoas

DeltasPessoas -> SigdnRhPessoasProvider : GetDeltas(timePeriod)
activate SigdnRhPessoasProvider

SigdnRhPessoasProvider -> SIGDN_WS : call SOAP ZhrWsGetDeltasPernr endpoint\n{Empresa, BegDate, EndDate}
activate SIGDN_WS

SIGDN_WS --> SigdnRhPessoasProvider : ZhrWsGetDeltasPernrOut[]
deactivate SIGDN_WS

SigdnRhPessoasProvider --> DeltasPessoas : PessoaImportKey[]
deactivate SigdnRhPessoasProvider

loop for each PessoaImportKey

    DeltasPessoas -> SigdnRhPessoasProvider : GetPessoasByImportKeysAsync(PessoaImportKey)
    activate SigdnRhPessoasProvider

    SigdnRhPessoasProvider -> SIGDN_WS : call SOAP Infotype endpoints\n{Empresa, Nii, Numsap}
    activate SIGDN_WS
    SIGDN_WS --> SigdnRhPessoasProvider : SOAP Infotype endpoints outputs[]
    SigdnRhPessoasProvider --> SigdnRhPessoasProvider : createPessoa()
    activate SigdnRhPessoasProvider
    deactivate SigdnRhPessoasProvider
    deactivate SIGDN_WS

    SigdnRhPessoasProvider --> DeltasPessoas : changedPessoa
    deactivate SigdnRhPessoasProvider


    DeltasPessoas -> DeltasPessoas : IsPessoaChanged(changedPessoa)
    activate DeltasPessoas
    DeltasPessoas -> PessoasRepository : GetExistingPessoaByNii(Nii)
    activate PessoasRepository

    PessoasRepository --> DeltasPessoas : Pessoa
    deactivate PessoasRepository

    DeltasPessoas -> DeltasPessoas : comparePessoas(Pessoa, changedPessoa)
    activate DeltasPessoas
    deactivate DeltasPessoas
    deactivate DeltasPessoas

    alt Pessoa Changed
        DeltasPessoas -> DeltasPessoas : Add to Upsert List
    else Pessoa Not Changed
        DeltasPessoas -> DeltasPessoas : Drop Pessoa (ignore)
    end

end

DeltasPessoas -> PessoasRepository : UpsertAllAsync(List<Pessoa>)
activate PessoasRepository
PessoasRepository --> DeltasPessoas : UpsertPessoasResult
deactivate PessoasRepository

DeltasPessoas --> PessoasDeltasController: DeltasPessoasResult
deactivate DeltasPessoas

PessoasDeltasController --> Admin : 202 Accepted (DeltasPessoasResult)
deactivate PessoasDeltasController

@enduml

```
