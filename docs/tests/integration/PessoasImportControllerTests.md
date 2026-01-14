# Casos de Teste de Integração: PessoasImportController

Este documento descreve os principais cenários de teste cobertos para o controller de importação de pessoas.

| Cenário                                                                                                        | Expectativa                                                                                                 |
| -------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------- |
| Importar pessoas do provider (mockado) com a BD vazia                                                          | Todas as pessoas do provider devem ser persistidas na BD                                                    |
| Importar pessoas do provider (mockado) contendo na BD apenas pessoas que não existem no provider               | Pessoas existentes na BD são preservadas e as do provider adicionadas                                       |
| Importar pessoas do provider (mockado) contendo na BD pessoas que existem e outras que não existem no provider | Pessoas existentes na BD são atualizadas, as que não existem preservadas e as novas do provider adicionadas |
| Importar pessoas do provider com resposta vazia                                                                | A BD permanece inalterada                                                                                   |
| Importar pessoas do provider (mockado) contendo na BD apenas pessoas só do provider                            | Pessoas existentes na BD são atualizadas e as novas adicionadas                                             |
| Importação com erro no serviço SOAP                                                                            | Retorna InternalServerError e a BD permanece vazia                                                          |
| Importação como viewer                                                                                         | Retorna Forbidden                                                                                           |
| Importação sem autenticação                                                                                    | Retorna Unauthorized                                                                                        |

> Para detalhes de implementação, consulte os métodos de teste em [`PessoasImportControllerTests.cs`](../../../tests/Pessoas.Integracao.Tests/Integration/Controllers/PessoasImportControllerTests.cs).
