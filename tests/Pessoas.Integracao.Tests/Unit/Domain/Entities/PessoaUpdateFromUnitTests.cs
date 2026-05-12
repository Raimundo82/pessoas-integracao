using System.Collections;

using FluentAssertions;

using Pessoas.Integracao.Core.Domain.Entities;
using Pessoas.Integracao.Core.Domain.Enums;


namespace Pessoas.Integracao.Tests.Unit.Domain.Entities;

public sealed class PessoaUpdateFromUnitTests
{

    [Theory]
    [ClassData(typeof(PessoaTestData))]
    public void ShouldUpdateAllProperties_WhenSourceHasValidData(Pessoa source, Pessoa existing)
    {
        // Act
        existing.UpdateFrom(source);

        // Assert
        existing.NII.Should().Be(source.NII);
        existing.ExternalId.Should().Be(source.ExternalId);
        existing.DadosPessoais.Should().BeEquivalentTo(source.DadosPessoais);
        existing.DadosBiometricos.Should().BeEquivalentTo(source.DadosBiometricos);
    }

    public class PessoaTestData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new Pessoa[] {
                new() { NII = "123", ExternalId = "Old" },
                new() { NII = "123" },
            };
            yield return new Pessoa[] {
                new() { NII = "123", ExternalId = "Old" },
                new() { NII = "123", ExternalId = "New" },
            };
            yield return new Pessoa[] {
                new() { NII = "123", ExternalId = "Old" },
                new() { NII = "123", ExternalId = "New", DadosPessoais = new(), DadosBiometricos = new() },
            };
            yield return new Pessoa[] {
                new() {
                    NII = "123",
                    ExternalId = "Old",
                    DadosPessoais = new()
                    {
                        NomeCompleto = "Nome Completo",
                        Apelidos = "Completo",
                        Sobrenome = "Comepleto",
                        DataNascimento = new DateOnly(2000, 10, 19),
                    },
                    DadosBiometricos = new()
                    {
                        AlturaEmCm = 180,
                        CorDosOlhos = "castanhos"
                    }
                },
                new() {
                    NII = "123",
                    ExternalId = "New",
                    DadosPessoais = new()
                    {
                        NomeCompleto = "Nome Completo Novo",
                        Apelidos = "Completo Novo",
                        Sobrenome = "Novo",
                        DataNascimento = new DateOnly(199, 10, 19),
                    },
                    DadosBiometricos = new()
                    {
                        AlturaEmCm = 178,
                        CorDosOlhos = "azuis",
                        TipoDeSangue = new() {
                            GrupoSanguineo = GrupoSanguineo.O,
                            Rhesus = Rhesus.POSITIVO}
                    }
                },
            };
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

}
