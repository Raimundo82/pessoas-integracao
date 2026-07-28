# Adoção do Riok.Mapperly para mapeamento de modelos no assembly Pessoas.Integracao.Analitica

Status: Proposto

## Contexto e Declaração do Problema

No assembly `Pessoas.Integracao.Analitica`, existe a necessidade de efetuar mapeamentos entre modelos de dados de diferentes camadas da aplicação, encontrando-se estes na camada de Infraestrutura. Pretende-se evitar código de mapeamento manual repetitivo (boilerplate) e garantir que os mapeamentos sejam validados durante a compilação. Foi definido o contrato genérico `IEntityMapper<TSource, TTarget>` para uniformizar a implementação dos mapeadores, com implementações concretas existentes no assembly (por exemplo, `Infrastructure/Mappers/AptidaoMapper.cs`).

## Opções Consideradas

- Mapeamento manual
- AutoMapper
- Mapster
- Riok.Mapperly

## Resultado da Decisão

**Opção escolhida:** "Riok.Mapperly", porque é uma biblioteca open-source e gratuita, sem custos de licenciamento, utiliza Source Generators para geração de código em compilação, com mapeamentos explícitos e validados durante o build, possui boas integrações com refactoring e análise estática, e mantém dependências mínimas em runtime.

### Consequências

#### Positivas

- Reduz código repetitivo (boilerplate).
- Garante consistência na implementação.
- Valida mapeamentos durante a compilação.
- Alinha-se com a arquitetura da solução.

#### Negativas

- Apresenta menor maturidade e adoção comparativamente ao AutoMapper.
- Menor flexibilidade para cenários extremamente dinâmicos.
- Requer familiaridade com Source Generators.
