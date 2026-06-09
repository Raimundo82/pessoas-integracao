
using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using Pessoas.Integracao.Tests.TestInfrastructure;
using Pessoas.Integracao.Worker.Infrastructure.Data;
using Pessoas.Integracao.Worker.Infrastructure.Models.Dados;
using Pessoas.Integracao.Worker.Infrastructure.Repositories.Aptidao;

namespace Pessoas.Integracao.Tests.Integration.Infrastructure.WorkerRepositories.Aptidao;


[Collection(nameof(PostgresTestDatabaseCollection))]
public sealed class ReplaceAllAptidoesAsyncDbIntegrationTests : IAsyncLifetime, IDisposable
{
    private readonly ZhrSDbContext _context;
    private readonly ZhrSAptidaoRepository _repository;
    private readonly DbContextOptions<ZhrSDbContext> _options;
    private readonly CancellationToken _ct = TestContext.Current.CancellationToken;
    private readonly PostgresTestContainerDb _db;


    public ReplaceAllAptidoesAsyncDbIntegrationTests(PostgresTestContainerDb db)
    {
        _db = db;
        _options = new DbContextOptionsBuilder<ZhrSDbContext>()
            .UseNpgsql(db.ConnectionString)
            .Options;

        _context = new ZhrSDbContext(_options);
        _repository = new ZhrSAptidaoRepository(_context);
    }
    public ValueTask InitializeAsync() => new(_db.ResetDatabaseAsync());

    [Fact]
    public async Task ShouldInsertAptidaoOutputAndNestedAptidoes_WhenDbIsEmpty()
    {
        // Arrange
        var aptidaoOutput = new ZhrSAptidaoOutput
        {
            Ni = "12345",
            Numsap = "54321",
        };

        var zhrSAptidaos = new ZhrSAptidao[]
        {
            new() { Root = aptidaoOutput },
            new() { Root = aptidaoOutput },
        };
        aptidaoOutput.Aptidao = zhrSAptidaos;

        // Act  
        await _repository.ReplaceAllAsync([aptidaoOutput], _ct);

        // Assert
        await using var verifyContext = new ZhrSDbContext(_options);

        var savedOutputs = await verifyContext.ZhrSAptidaoOutputs.ToListAsync(_ct);
        savedOutputs.Should().HaveCount(1);
        var savedOutput = savedOutputs.Single();
        savedOutput.Ni.Should().Be("12345");
        savedOutput.Numsap.Should().Be("54321");

        var savedAptidoes = await verifyContext.ZhrSAptidoes.ToListAsync(_ct);
        savedAptidoes.Should().HaveCount(2);
        savedAptidoes.Should().AllSatisfy(a => a.ZhrSAptidaoOutputId.Should().Be(savedOutput.Id));
    }


    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
