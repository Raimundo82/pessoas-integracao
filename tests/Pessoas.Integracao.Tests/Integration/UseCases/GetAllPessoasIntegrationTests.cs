using System.Collections.ObjectModel;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using Pessoas.Integracao.Core.Application.DTOs;
using Pessoas.Integracao.Core.Application.UseCases;
using Pessoas.Integracao.Core.Domain.Entities;
using Pessoas.Integracao.Core.Domain.Enums;
using Pessoas.Integracao.Core.Domain.ValueObjects;
using Pessoas.Integracao.Core.Infrastructure.Data;
using Pessoas.Integracao.Core.Infrastructure.Repositories;
using Pessoas.Integracao.Tests.TestInfrastructure;

namespace Pessoas.Integracao.Tests.Integration.UseCases;


[Collection(nameof(PostgresTestDatabaseCollection))]
public sealed class GetAllPessoasIntegrationTests : IAsyncLifetime, IDisposable
{
    private readonly AppDbContext _context;
    private readonly PessoaRepository _repository;
    private readonly PostgresTestContainerDb _db;


    public GetAllPessoasIntegrationTests(PostgresTestContainerDb db)
    {
        _db = db;
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(db.ConnectionString)
            .Options;

        _context = new AppDbContext(options);
        _repository = new PessoaRepository(_context);

    }
    public ValueTask InitializeAsync() => new(_db.ResetDatabaseAsync());

    [Fact]
    public async Task ExecuteAsync_WhenPessoasExist_ReturnsAllPessoaDtos()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
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
                    DataNascimento = new DateOnly(1982, 10, 18),
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
        _context.AddRange(pessoas);
        await _context.SaveChangesAsync(ct);
        _context.ChangeTracker.Clear();
        var useCase = new GetAllPessoas(_repository);


        // Act
        var result = await useCase.ExecuteAsync(ct);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeAssignableTo<IReadOnlyList<PessoaDto>>();
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo([new PessoaDto("22600", "30002697"), new PessoaDto("21200", "30002797")]);
    }


    [Fact]
    public async Task ExecuteAsync_WhenNoPessoasExist_ReturnsEmptyCollection()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var useCase = new GetAllPessoas(_repository);

        // Act (When)
        var result = await useCase.ExecuteAsync(ct);

        // Assert (Then)
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
