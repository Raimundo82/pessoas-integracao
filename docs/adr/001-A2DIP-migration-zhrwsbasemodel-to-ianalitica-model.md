# Migração de `ZhrWsBaseModel` para `IAnaliticaModel` e `AnaliticaBaseModel`

Status: Aceite

## Contexto e Declaração do Problema

O projeto `Pessoas.Integracao.Analitica` utiliza modelos gerados automaticamente pelo EF Core Power Tools para representar as tabelas da base de dados analítica. Estes modelos gerados são classes parciais (`partial class`) que herdam de uma classe base comum, originalmente denominada `ZhrWsBaseModel`.

A classe `ZhrWsBaseModel` era uma classe abstrata que definia as propriedades comuns:

- `Id` (int)
- `Ni` (string)
- `UpdatedAt` (DateTimeOffset?)

**Nota sobre a Mudança de Nome**: O nome `ZhrWsBaseModel` foi alterado para `AnaliticaBaseModel` para evitar confusão com o nome `ZhrWs...` usado pelo sistema externo SIGDN-RH e pelo projeto `Pessoas.Integracao.Sync` que faz a sincronização dos dados desse sistema externo.

Com a evolução do sistema, surgiram as seguintes necessidades:

1. **Mudança de Tipo para `UpdatedAt`**: A propriedade `UpdatedAt` foi alterada para `DateTimeOffset?` para garantir consistência e uniformização com o restante projeto, precisão temporal e compatibilidade com fusos horários.

2. **Necessidade de Contratos Centralizados**: É necessário ter um contrato (interface) que permita aceder a propriedades como `Numsap` de forma centralizada em todas as classes concretas, sem necessidade de fazer cast para o tipo concreto.

3. **Conflito com Classes Geradas**: As classes concretas geradas pelo EF Core Power Tools (ex: `ZhrWsAptidaoAptidao`, `ZhrWsAtribOrgAtribOrg`) já possuem a propriedade `Numsap` definida nos ficheiros gerados em `Models/Generated/`. Adicionar `Numsap` à classe base abstrata causaria um conflito de duplicação de propriedades.

4. **Manutenibilidade das múltiplas classes concretas ZhrWs**: Adicionar os atributos comuns (`Id`, `Ni`, `UpdatedAt`) individualmente a cada uma das classes concretas ZhrWs geradas seria anti-pattern e dificultaria a manutenção.

## Opções Consideradas

- Manter uma Classe Abstrata Única `AnaliticaBaseModel` e adicionar `Numsap` a esta classe.
- Adicionar Propriedades Individualmente a Cada Classe Concreta.
- Usar Apenas a Interface `IAnaliticaModel` Sem Classe Abstrata.
- Criação da Interface `IAnaliticaModel` e da Classe Abstrata `AnaliticaBaseModel` com Classes Parciais Concretas.

## Resultado da Decisão

**Opção escolhida:** "Criação da Interface `IAnaliticaModel` e da Classe Abstrata `AnaliticaBaseModel` com Classes Parciais Concretas", porque permite ter um contrato centralizado para `Numsap`, reutiliza propriedades comuns sem conflitos com as classes geradas automaticamente pelo EF Core Power Tools, e mantém a tipagem forte com `DateTimeOffset?` para `UpdatedAt`.

Adotou-se a seguinte arquitetura para os modelos da camada Analitica:

1. **Criação da Interface `IAnaliticaModel`**:

   ```csharp
   public interface IAnaliticaModel
   {
       public int Id { get; set; }
       public string Ni { get; set; }
       public string? Numsap { get; set; }
       public DateTimeOffset? UpdatedAt { get; set; }
   }
   ```

2. **Criação da Classe Abstrata `AnaliticaBaseModel`**:

   ```csharp
   public abstract class AnaliticaBaseModel
   {
       public int Id { get; set; }
       public required string Ni { get; set; }
       public DateTimeOffset? UpdatedAt { get; set; }
   }
   ```

3. **Atualização das Classes Parciais Concretas**:
   As classes parciais geradas mantêm as suas propriedades específicas (incluindo `Numsap`), e as declarações parciais adicionais herdam de `AnaliticaBaseModel` e implementam `IAnaliticaModel`:

   ```csharp
   public partial class ZhrWsAptidaoAptidao : AnaliticaBaseModel, IAnaliticaModel { }
   public partial class ZhrWsAtribOrgAtribOrg : AnaliticaBaseModel, IAnaliticaModel { }
   ```

4. **Atualização do `AnaliticaDbContext`**:
   O método `OnModelCreatingPartial` foi atualizado para usar verificação de interface em vez de herança:

   ```csharp
   foreach (var entityType in modelBuilder.Model.GetEntityTypes()
       .Where(t =>
           typeof(IAnaliticaModel).IsAssignableFrom(t.ClrType) &&
           t.ClrType.IsClass &&
           !t.ClrType.IsAbstract
           )
   )
   ```

5. **Atualização do Repositório e Interface**:
   A interface `IAnaliticaRepository<TEntity>` e a implementação `AnaliticaRepository<TEntity>` foram atualizadas para usar `where TEntity : class, IAnaliticaModel` com `AnaliticaBaseModel` como classe base concreta, em vez de usar diretamente a classe anterior `ZhrWsBaseModel`.

### Consequências

#### Positivas

- **Contrato Centralizado para `Numsap`**: A interface `IAnaliticaModel` permite aceder a `Numsap` de forma centralizada em todas as entidades analíticas, sem necessidade de cast para tipos concretos.
- **Reutilização de Propriedades Comuns**: A classe `AnaliticaBaseModel` fornece `Id`, `Ni` e `UpdatedAt` sem necessidade de os adicionar individualmente a cada uma das 30+ classes concretas.
- **Compatibilidade com Geração Automática**: O padrão de classes parciais permite que as classes geradas pelo EF Core Power Tools mantenham as suas propriedades específicas sem conflitos de duplicação.
- **Tipagem Forte com `DateTimeOffset?`**: A propriedade `UpdatedAt` agora usa `DateTimeOffset?`, garantindo precisão temporal e compatibilidade com fusos horários.
- **Flexibilidade no DbContext**: O uso de `typeof(IAnaliticaModel).IsAssignableFrom(t.ClrType) && t.ClrType.IsClass && !t.ClrType.IsAbstract` permite que qualquer classe que implemente a interface e seja uma classe concreta seja incluída no model building.

#### Negativas

- **Complexidade Adicional**: A combinação de interface + classe abstrata + classes parciais gera uma arquitetura ligeiramente mais complexa de compreender para novos elementos.
- **Risco de Conflicto com Geração Automática**: As declarações parciais adicionais (`public partial class ZhrWsAptidaoAptidao : AnaliticaBaseModel, IAnaliticaModel { }`) devem ser mantidas fora da pasta `Generated/` para não serem sobrescritas pelo EF Core Power Tools.
