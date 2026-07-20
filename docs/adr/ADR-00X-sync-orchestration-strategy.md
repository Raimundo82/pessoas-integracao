# ADR-00X: Estratégia de orquestração da ingestão de dados do SIGDN-RH

## Status

Accepted

## Contexto

O componente Sync necessita de obter dados de múltiplos webservices do SIGDN-RH, normalizar os outputs e persistir os dados no schema de origem. É necessário decidir como organizar estas responsabilidades dentro do Orquestrador e onde vive a lógica de resolução de Ni, UpdatedAt e agregação de children.

## Decisão

### 1. Orquestração "vertical" por webservice

Cada webservice é encapsulado num `IZhrSynchronizer` que coordena internamente o fetch, resolve o Ni e a data de atualização e persiste. O Orquestrador `ZhrSyncOrchestrator` executa os synchronizers sequencialmente ou de forma concorrente, dependendo da capacidade do SIGDN-RH em lidar com pedidos simultâneos.

### 2. Lógica de resolução na hierarquia de classes

A lógica de injecção de `Ni` nos children e marcação de `UpdatedAt` no root vive na classe base `ZhrSBaseModelOutput` através de três métodos:

- `SetUpdatedAt()` — marca o timestamp de actualização no root
- `GetChildren()` — abstracto, cada output concreto implementa devolvendo a sua estrutura de children
- `SetNi()` — injeta o `Ni` em todos os children usando `GetChildren()`

Cada partial concreta implementa apenas `GetChildren()`, sendo essa a sua única responsabilidade: conhecer e devolver a sua própria estrutura de children.

### 3. Agregação de children por tipo

Antes de persistir, os children de todos os outputs têm de ser agregados por tipo, sendo que cada tabela na BD corresponde a um tipo de child, não a um output. Para esta agregação, optou-se por um componente dedicado:

**Componente `ZhrChildrenAggregator`** — A lógica de agregação é extraída para um componente injectado no synchronizer. Testável isoladamente e mantém o synchronizer focado na orquestração.

**Motivação para a escolha:**

- A agregação por tipo é uma lógica reutilizável entre synchronizers
- Isola a complexidade de agrupamento e flattening num componente testável
- Mantém o synchronizer focado na orquestração (fetch + persistência)
- Facilita evolução futura se a lógica de agregação se tornar mais complexa

### 4. Classe base abstrata para synchronizers

Foi introduzida a classe `ZhrSynchronizerBase` que contém a dependência do `IZhrChildrenAggregator` e disponibiliza o método `AggregateChildren()` para uso pelos synchronizers concretos.

## Diagramas

### Diagrama de Classes

```plantuml
@startuml Class Diagram

title Class Diagram — Orquestração da ingestão de dados do SIGDN-RH

interface IZhrSynchronizer {
    +ExecuteAsync(refs): Task
}

interface IZhrChildrenAggregator {
    +Aggregate(outputs: IEnumerable<ZhrSBaseModelOutput>): List<ZhrSBaseModel[]>
}

abstract class ZhrSBaseModelOutput {
    +SetUpdatedAt(): void
    +SetNi(): void
    +GetChildren(): IReadOnlyList<ZhrSBaseModel[]>
}

abstract class ZhrSynchronizerBase {
    -childrenAggregator: IZhrChildrenAggregator
    +ExecuteAsync(refs): Task
    #AggregateChildren(outputs): List<ZhrSBaseModel[]>
}

class ZhrSyncOrchestrator {
    -synchronizers: IEnumerable<IZhrSynchronizer>
    +ExecuteAsync(refs): Task
}

class ZhrAptidaoSynchronizer {
    -client: IZhrGenericClient
    -persister: IZhrPersistenceReplacer
    +ExecuteAsync(refs): Task
}

class ZhrPersonalDataSynchronizer {
    -client: IZhrGenericClient
    -persister: IZhrPersistenceReplacer
    +ExecuteAsync(refs): Task
}

class ZhrChildrenAggregator {
    +Aggregate(outputs: IEnumerable<ZhrSBaseModelOutput>): List<ZhrSBaseModel[]>
}

class ZhrSAptidaoOutput {
    +GetChildren(): IReadOnlyList<ZhrSBaseModel[]>
}

class ZhrSPersonalDataOutput {
    +GetChildren(): IReadOnlyList<ZhrSBaseModel[]>
}

ZhrSyncOrchestrator o-- IZhrSynchronizer
IZhrSynchronizer <|.. ZhrSynchronizerBase
ZhrSynchronizerBase <|-- ZhrAptidaoSynchronizer
ZhrSynchronizerBase <|-- ZhrPersonalDataSynchronizer
ZhrSynchronizerBase --> IZhrChildrenAggregator
IZhrChildrenAggregator <|.. ZhrChildrenAggregator
ZhrSBaseModelOutput <|-- ZhrSAptidaoOutput
ZhrSBaseModelOutput <|-- ZhrSPersonalDataOutput

@enduml
```

```plantuml
@startuml Sequência

title Orquestração da ingestão de dados do SIGDN-RH

participant ZhrSyncOrchestrator as Orchestrator
participant "ZhrAptidaoSynchronizer" as Sync1
participant "ZhrPersonalDataSynchronizer" as Sync2
participant ZhrChildrenAggregator as Aggregator
participant SIGDN
database BD

Orchestrator -> Sync1: ExecuteAsync(refs) (Task 1)
activate Sync1

Orchestrator -> Sync2: ExecuteAsync(refs) (Task 2)
activate Sync2

note over Orchestrator: Task.WhenAll(Sync1, Sync2)

Sync1 -> SIGDN: request
SIGDN --> Sync1: output

Sync1 -> Sync1: output.SetUpdatedAt()
Sync1 -> Sync1: output.SetNi()

Sync1 -> Aggregator: Aggregate(outputs)
activate Aggregator
Aggregator --> Sync1: List<ZhrSBaseModel[]>
deactivate Aggregator

Sync1 -> BD: persist root + children
BD --> Sync1: ok

Sync1 --> Orchestrator: done
deactivate Sync1

Sync2 -> SIGDN: request
SIGDN --> Sync2: output

Sync2 -> Sync2: output.SetUpdatedAt()
Sync2 -> Sync2: output.SetNi()

Sync2 -> Aggregator: Aggregate(outputs)
activate Aggregator
Aggregator --> Sync2: List<ZhrSBaseModel[]>
deactivate Aggregator

Sync2 -> BD: persist root + children
BD --> Sync2: ok

Sync2 --> Orchestrator: done
deactivate Sync2

@enduml
```

## Alternativas rejeitadas

**Componente externo `IZhrOutputResolver`** — rejeitado porque a lógica de injecção de `Ni` e marcação de `UpdatedAt` pertence ao modelo. `SetNi()` no root usa `GetChildren()` para injectar o `Ni` sem duplicação e sem componentes externos. As classes parciais permitem adicionar este comportamento sem tocar no código gerado pelo WCF.

**`GetChildren()` com lógica de `Ni` interna** — rejeitado porque mistura duas responsabilidades numa só operação. `GetChildren()` deve apenas devolver a estrutura de children, sendo a injecção do `Ni` responsabilidade de `SetNi()` no root.

**Agregação inline no synchronizer** — rejeitada porque a lógica de agregação por tipo é reutilizável entre synchronizers e merece um componente dedicado para melhor testabilidade e manutenção.

**Orquestração por Componentes Independentes** — rejeitada porque não existe uma forma limpa de associar `ZhrProvider1` ao `ZhrOutputResolver1` ao `ZhrPersister1` sem mecanismos de correlação frágeis. O encapsulamento no `IZhrSynchronizer` resolve este problema naturalmente.

## Consequências

- Novo webservice SIGDN acarreta novo `IZhrSynchronizer` + partial com `GetChildren()`, registado como DI
- `ZhrSyncOrchestrator` mantém-se estável independentemente do número de webservices
- `IZhrGenericClient` e `IZhrPersisterReplacer` são componentes partilhados injectados em cada synchronizer
- Testabilidade por synchronizer via mock de `IZhrGenericClient` e `IZhrPersisterReplacer`
- `GetChildren()` testável directamente nas classes do modelo sem dependências externas
- `ZhrChildrenAggregator` testável isoladamente
- Agregação de children por tipo é responsabilidade do `ZhrChildrenAggregator`, chamado pelo synchronizer antes de persistir

## Concorrência e Fiabilidade

### Concorrência e Fiabilidade

A estratégia de orquestração recorre à execução paralela dos sincronizadores através de `Task.WhenAll`. Esta abordagem é segura e eficiente, com base nas seguintes premissas:

- Isolamento de Dados: Cada ZhrSyncronizer gere um grafo de entidades completamente independente. Não existem dependências cruzadas ou restrições de chaves estrangeiras entre as raízes processadas por diferentes instâncias do ZhrSyncronizer, eliminando assim o risco de race conditions na integridade referencial.

- Gestão de Recursos: Com um máximo de 20 entidades root que correspondem aos webservices existentes, a concorrência máxima está limitada a 20 ligações simultâneas à base de dados. Este valor encontra-se bem abaixo do limite configurado para o pool de ligações (50), garantindo que não ocorrem timeouts de ligação durante o pico de sincronização.

- Estratégia de Escrita: Para evitar conflitos de "Insert ou Update", o sistema utiliza um padrão de "Eliminação seguida de Inserção" (Delete-then-Insert). Isto assegura um estado limpo para cada conjunto de entidades e previne a violação de chaves primárias durante as escritas concorrentes.
