# Domain model

## Software engineering conventions

- Aggregate root: `Pessoa`
- Canonical domain identity: `NII` (unique)
- Integration identifier: `externalId` (not canonical for domain rules)
- Temporal model:
  - `dataNascimento` is a date-only value
  - `inicio` and `fim` are date-only validity boundaries
  - `fim` is optional for open-ended periods
- Invariants:
  - `NII` is mandatory and unique
  - if `fim` exists, then `fim >= inicio`
  - value objects are immutable and compared by value
  - UnidadeExterna `externalId` is the external system reference and must be unique

```plantuml
@startuml domain-model-pessoas

title Domain Model - Pessoas (software baseline)

package "Domain" #DDDDDD {

entity Pessoa {
  NII : string
  externalId : string
  dadosPessoais : DadosPessoais
  dadosBiometricos : DadosBiometricos
  colocacoes : Colocacao[]
  vinculos : Vinculo[]
}

class DadosPessoais << ValueObject >> {
  nomeCompleto : string
  sobrenome : string
  apelidos : string
  dataNascimento : Date
}

class DadosBiometricos << ValueObject >> {
  corDosOlhos : string
  alturaEmCm : string
  tipoDeSangue : TipoDeSangue
}

abstract Vinculo {
  inicio : Date
  fim : Date?
}

class VinculoMilitar {}
class VinculoMilitarizado {}
class VinculoCivil {}

class Colocacao {
  inicio : Date
  fim : Date?
  externalReference : UnidadeExternaRef
}

class UnidadeExternaRef << ValueObject >> {
  externalReference : string
}

class TipoDeSangue << ValueObject >> {
  grupoDeSangue : GrupoDeSangue
  rhesus : Rhesus
}

enum GrupoDeSangue {
  A
  B
  AB
  O
}

enum Rhesus {
  POSITIVO
  NEGATIVO
}

class Infrastructure.UnidadeExterna {
  externalId : string
  nome : string?
}

Pessoa : {unique(NII)}

Pessoa *-- "1" DadosPessoais
Pessoa *-- "1" DadosBiometricos
Pessoa *-- "0..*" Colocacao
Pessoa *-- "0..*" Vinculo

Vinculo <|-- VinculoMilitar
Vinculo <|-- VinculoMilitarizado
Vinculo <|-- VinculoCivil

Colocacao --> "1" UnidadeExternaRef
Colocacao ..> Infrastructure.UnidadeExterna : lookup by ExternalId

DadosBiometricos --> "1" TipoDeSangue

TipoDeSangue --> "1" GrupoDeSangue
TipoDeSangue --> "1" Rhesus

Infrastructure.UnidadeExterna : {unique(externalId)}
}

@enduml
```
