
using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using Pessoas.Integracao.Tests.TestInfrastructure;
using Pessoas.Integracao.Worker.Infrastructure.Data;
using Pessoas.Integracao.Worker.Infrastructure.Models.Dados;
using Pessoas.Integracao.Worker.Infrastructure.Repositories.Aptidao;

namespace Pessoas.Integracao.Tests.Integration.Infrastructure.WorkerRepositories.Aptidao;


[Collection(nameof(PostgresTestDatabaseCollection))]
public sealed class GetAllAptidoesAsyncDbIntegrationTests : IAsyncLifetime, IDisposable
{
    private readonly ZhrSDbContext _context;
    private readonly ZhrSAptidaoRepository _repository;
    private readonly DbContextOptions<ZhrSDbContext> _options;
    private readonly CancellationToken _ct = TestContext.Current.CancellationToken;
    private readonly PostgresTestContainerDb _db;


    public GetAllAptidoesAsyncDbIntegrationTests(PostgresTestContainerDb db)
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
    public async Task ShouldReturnAptidaoOutputAndAllNestedAptidoes_WhenDbHasASingleAptidaoOutput()
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
        await SeedAsync(aptidaoOutput);


        // Act  
        var result = await _repository.GetAllAsync(_ct);

        // Assert
        result.Should().HaveCount(1);
        var aptidoesResult = result.Single().Aptidao;
        aptidoesResult.Should().HaveCount(2);
        aptidoesResult.Should().AllSatisfy(a => a.ZhrSAptidaoOutputId.Should().Be(result.Single().Id));
    }

    private async Task<List<ZhrSAptidaoOutput>> SeedAsync(params ZhrSAptidaoOutput[] aptidaoOutputs)
    {
        await using var seedContext = new ZhrSDbContext(_options);
        await seedContext.ZhrSAptidaoOutputs.AddRangeAsync(aptidaoOutputs);
        await seedContext.SaveChangesAsync();

        var children = aptidaoOutputs
            .SelectMany(o => o.Aptidao.Select(a => { a.ZhrSAptidaoOutputId = o.Id; return a; }))
            .ToList();

        if (children.Count > 0)
        {
            await seedContext.ZhrSAptidoes.AddRangeAsync(children);
            await seedContext.SaveChangesAsync();
        }

        return [.. aptidaoOutputs];
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
