namespace Pessoas.Integracao.Tests.Unit.Application;

using System.Collections.ObjectModel;

using FluentAssertions;

using Moq;

using Pessoas.Integracao.Core.Application.DTOs;
using Pessoas.Integracao.Core.Application.UseCases;
using Pessoas.Integracao.Core.Domain.Entities;
using Pessoas.Integracao.Core.Domain.Enums;
using Pessoas.Integracao.Core.Domain.Interfaces;
using Pessoas.Integracao.Core.Domain.ValueObjects;

public sealed class GetAllPessoasUnitTests : IDisposable
{
    private Mock<IPessoaRepository> _repo;

    public GetAllPessoasUnitTests()
    {
        // Setup runs before each test
        _repo = new Mock<IPessoaRepository>();
    }

    [Fact]
    public async Task ExecuteAsync_WhenPessoasExist_ReturnsAllPessoaDtos()
    {
        // Arrange (Given)
        var pessoas = new ReadOnlyCollection<Pessoa>(
        [
            new() {
                Id = 1,
                NII = "22600",
                ExternalId = "30002697",
                DadosPessoais = new DadosPessoais {
                    Apelidos = "Apelidos",
                    NomeCompleto = "Nome Completo",
                    Sobrenome = "Sobrenome",
                    DataNascimento = new DateOnly(1982, 10, 18)
                },
                DadosBiometricos = new DadosBiometricos {
                    AlturaEmCm = 176,
                    CorDosOlhos = "Castanhos",
                    TipoDeSangue = new TipoDeSangue
                    {
                        GrupoSanguineo = GrupoSanguineo.O,
                        Rhesus = Rhesus.POSITIVO
                    }
                }
            },
            new() { Id = 2, NII = "21200", ExternalId = "30002797" }
        ]);

        _repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
           .ReturnsAsync(pessoas);

        var uut = new GetAllPessoas(_repo.Object);

        // Act (When)
        var result = await uut.ExecuteAsync(CancellationToken.None);

        // Assert (Then)
        result.Should().NotBeNull();
        result.Should().BeAssignableTo<IReadOnlyCollection<PessoaDto>>();
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo([new PessoaDto("22600", "30002697"), new PessoaDto("21200", "30002797")]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoPessoasExist_ReturnsEmptyCollection()
    {
        // Arrange (Given)
        _repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync([]);
        var uut = new GetAllPessoas(_repo.Object);

        // Act (When)
        var result = await uut.ExecuteAsync(CancellationToken.None);

        // Assert (Then)
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    public void Dispose()
    {
        _repo = null!;
        GC.SuppressFinalize(this);
    }
}