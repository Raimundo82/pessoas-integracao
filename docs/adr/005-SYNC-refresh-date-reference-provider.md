# Obtenção da Data de Referência de Atualização via Injeção de Dependência

Status: Proposto

## Contexto e Declaração do Problema

Durante a execução da sincronização dos dados referentes às `PessoaSyncRef`, vários componentes intervenientes no processo (como o `IZhrFreshnessChecker`, que verifica se os dados estão atualizados com base numa data de referência, e o `ZhrBaseModelOutputEnricher` que enriquece os dados recém-obtidos com essa data durante o processo de sincronização) necessitam de obter a data de referência de atualização. Inicialmente, considerou-se passar esta data como parâmetro explícito nos métodos dos sincronizadores.

No entanto, a data de referência é um dado intrínseco ao contexto de execução (seja uma tarefa agendada, _cron job_, ou um pedido HTTP), e passá-la como parâmetro através de várias camadas cria acoplamento e torna as assinaturas dos métodos verbosas e ruidosas. É necessário um mecanismo padrão para que os componentes a obtenham de forma desacoplada, sem que os orquestradores ou _callers_ tenham de a passar explicitamente através de várias camadas.

## Opções Consideradas

- Passar a data de referência como parâmetro explícito em todos os métodos de processamento e interfaces relacionadas com a sincronização.
- Utilizar um objeto de contexto global ou estático (`AsyncLocal<T>`) para armazenar e obter a data de referência em qualquer ponto do fluxo de execução.
- Utilizar Injeção de Dependência (DI) com um serviço scoped (`IZhrRefreshReferenceDateProvider`) definido explicitamente no início do fluxo de execução (no orquestrador, seja HTTP ou tarefa agendada) e obtido pelos componentes que necessitam da data através da sua injeção no construtor.

## Resultado da Decisão

**Opção escolhida:** Injeção de Dependência (DI) com um serviço _scoped_ (`IZhrRefreshReferenceDateProvider`), porque mantém as assinaturas dos métodos limpas, permite que cada componente injete apenas as dependências que realmente necessita, e alinha-se com os princípios de _Clean Architecture_ para dados de contexto de execução.

`AsyncLocal<T>` foi rejeitado apesar de também manter assinaturas limpas: torna a dependência de cada componente na data de referência invisível na sua assinatura (é um _ambient context_), e o seu ciclo de vida é mais difícil de gerir e testar de forma previsível do que um serviço scoped, cujo tempo de vida é explicitamente controlado pelo `IServiceScope`.

### Design

O acesso é dividido em duas interfaces implementadas pela mesma classe concreta scoped: uma pública, só de leitura, consumida pelos componentes de associados ao processo de sincronização; e uma de escrita, usada apenas pelos orquestradores para definir a data uma única vez no início do fluxo.

```csharp
public interface IZhrRefreshReferenceDateProvider
{
    DateTimeOffset GetReferenceDate();
}

internal interface IZhrUpdateReferenceDateInitializer
{
    void SetReferenceDate(DateTimeOffset referenceDate);
}
```

Ambas as interfaces devem ser implementadas pela mesma classe concreta e registadas apontando para a mesma instância scoped no container de DI, de forma a que todos os componentes resolvidos (direta ou transitivamente) a partir do mesmo scope partilhem a mesma data já configurada.

A implementação deve garantir três invariantes:

- **Normalização:** `SetReferenceDate(...)` normaliza o valor recebido para meia-noite UTC, conforme o ADR [002-GEN](./002-GEN-explicit-midnight-for-daily-granularity.md) sobre granularidade diária de `DateTimeOffset`, tirando a responsabilidade dos _callers_ de truncar a hora manualmente.
- **Falha explícita:** `GetReferenceDate()` lança `InvalidOperationException` se chamado antes de `SetReferenceDate(...)`; uma segunda chamada a `SetReferenceDate(...)` no mesmo scope também falha explicitamente. Isto evita que um erro de configuração no orquestrador resulte numa decisão silenciosa e incorreta de _freshness_, em vez de um valor por omissão mascarado.
- **Definição explícita por orquestrador:** tanto o controlador HTTP como o serviço de tarefa agendada devem chamar `SetReferenceDate(...)` explicitamente no início do fluxo (nunca através de um _middleware_ implícito), antes de resolver o serviço ou caso de uso associado ao processo de sincronização. No caso HTTP, o scope é fornecido automaticamente pelo _framework_ por pedido; na tarefa agendada, deve ser criado manualmente com `IServiceScopeFactory.CreateScope()`.

**Nota:** à data da elaboração deste ADR, como todos os componentes consumidores pertencem ao mesmo assembly, a separação entre as duas interfaces não é imposta pelo compilador, funcionando como convenção de design reforçada por _code review_, não como garantia técnica.

### Consequências

#### Positivas

- **Desacoplamento e assinaturas limpas:** os componentes associados ao processo de sincronização não precisam de conhecer a origem do contexto de execução, nem de receber a data como parâmetro adicional.
- **Testabilidade:** fácil de simular com `Mock<IZhrRefreshReferenceDateProvider>` em testes unitários.
- **Segregação por convenção:** a separação leitura/escrita comunica claramente a intenção de uso, ainda que não seja imposta pelo compilador enquanto tudo residir no mesmo assembly.
- **Falha rápida:** erros de configuração (data em falta ou definida duas vezes) são detetados imediatamente, em vez de silenciosamente.

#### Negativas

- **Configuração explícita no orquestrador:** cada orquestrador (controlador ou tarefa agendada) precisa de invocar `SetReferenceDate(...)` no início do fluxo.
- **Gestão de scope em tarefas agendadas:** requer criação manual de `IServiceScope` por execução, ao contrário do pipeline HTTP.
- **Segregação não imposta pelo compilador:** nada impede tecnicamente que um componente associado ao processo de sincronização injete a interface de escrita, sendo que a garantia depende de convenção e de _code review_.
