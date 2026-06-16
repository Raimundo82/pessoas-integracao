using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using Npgsql;

using Pessoas.Integracao.Analitica.Infrastructure.Data;
using Pessoas.Integracao.Analitica.Infrastructure.Repositories;
using Pessoas.Integracao.Analitica.Models;
using Pessoas.Integracao.Tests.TestInfrastructure;

namespace Pessoas.Integracao.Tests.Integration.Infrastructure.AnaliticaRepositories;

[Collection(nameof(PostgresTestDatabaseCollection))]
public sealed class AnaliticaRepositoryIntegrationTests : IAsyncLifetime, IDisposable
{
    private readonly AnaliticaDbContext _context;
    private readonly DbContextOptions<AnaliticaDbContext> _options;
    private readonly CancellationToken _ct = TestContext.Current.CancellationToken;
    private readonly PostgresTestContainerDb _db;

    public AnaliticaRepositoryIntegrationTests(PostgresTestContainerDb db)
    {
        _db = db;
        _options = new DbContextOptionsBuilder<AnaliticaDbContext>()
            .UseNpgsql(db.ConnectionString)
            .Options;

        _context = new AnaliticaDbContext(_options);
    }

    private AnaliticaRepository<T> GetRepository<T>() where T : ZhrWsBaseModel
        => new(_context);

    private async Task<List<T>> GetAll<T>() where T : ZhrWsBaseModel
    {
        await using var context = new AnaliticaDbContext(_options);
        return await context.Set<T>().ToListAsync();
    }

    public ValueTask InitializeAsync() => new(_db.ResetDatabaseAsync());

    [Fact]
    public async Task ReplaceMatchingByNiAsync_ShouldPreserveAllRows_WhenInputIsEmpty()
    {
        // Arrange
        var repository = GetRepository<ZhrWsAptidaoAptidao>();
        var existing = new[]
        {
            new ZhrWsAptidaoAptidao { Ni = "20002", Subty = "0001" },
            new ZhrWsAptidaoAptidao { Ni = "20102", Subty = "0001" }
        };
        await _context.Set<ZhrWsAptidaoAptidao>().AddRangeAsync(existing);
        await _context.SaveChangesAsync(_ct);

        // Act
        await repository.ReplaceMatchingByNiAsync([], _ct);

        // Assert
        var result = await GetAll<ZhrWsAptidaoAptidao>();
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task ReplaceMatchingByNiAsync_ShouldReplaceMatchingRows_WhenNiExists()
    {
        // Arrange
        var repository = GetRepository<ZhrWsAptidaoAptidao>();
        await _context.Set<ZhrWsAptidaoAptidao>().AddRangeAsync(
            new ZhrWsAptidaoAptidao { Ni = "20002", Subty = "Old" },
            new ZhrWsAptidaoAptidao { Ni = "20102", Subty = "Keep" }
        );
        await _context.SaveChangesAsync(_ct);

        var updates = new[] { new ZhrWsAptidaoAptidao { Ni = "20002", Subty = "New" } };

        // Act
        await repository.ReplaceMatchingByNiAsync(updates, _ct);

        // Assert
        var result = await GetAll<ZhrWsAptidaoAptidao>();
        result.Should().HaveCount(2);
        result.Should().ContainSingle(e => e.Ni == "20002" && e.Subty == "New");
        result.Should().ContainSingle(e => e.Ni == "20102" && e.Subty == "Keep");
    }

    [Fact]
    public async Task ReplaceAllAsync_ShouldClearTableAndInsertNew()
    {
        // Arrange
        var repository = GetRepository<ZhrWsAptidaoAptidao>();
        await _context.Set<ZhrWsAptidaoAptidao>().AddAsync(new ZhrWsAptidaoAptidao { Ni = "1", Subty = "Old" }, _ct);
        await _context.SaveChangesAsync(_ct);

        var newData = new[] { new ZhrWsAptidaoAptidao { Ni = "2", Subty = "New" } };

        // Act
        await repository.ReplaceAllAsync(newData, _ct);

        // Assert
        var result = await GetAll<ZhrWsAptidaoAptidao>();
        result.Should().ContainSingle().Which.Ni.Should().Be("2");
    }

    [Fact]
    public async Task ReplaceMatchingByNiAsync_ShouldRollback_WhenErrorOccurs()
    {
        // Arrange
        var repository = GetRepository<ZhrWsAptidaoAptidao>();
        await _context.Set<ZhrWsAptidaoAptidao>().AddAsync(new ZhrWsAptidaoAptidao { Ni = "999", Subty = "Safe" }, _ct);
        await _context.SaveChangesAsync(_ct);

        var invalidData = new[] { new ZhrWsAptidaoAptidao { Ni = null!, Subty = "Error" } };

        // Act
        Func<Task> act = async () => await repository.ReplaceMatchingByNiAsync(invalidData, _ct);

        // Assert
        await act.Should().ThrowAsync<PostgresException>();
        var result = await GetAll<ZhrWsAptidaoAptidao>();
        result.Should().ContainSingle(e => e.Ni == "999");
    }

    [Fact]
    public async Task ReplaceMatchingByNiAsync_ShouldWorkWithDifferentGenericType()
    {
        // Arrange
        var repository = GetRepository<ZhrWsAtribOrgAtribOrg>();
        var data = new[] { new ZhrWsAtribOrgAtribOrg { Ni = "100", Unid = "Test" } };

        // Act
        await repository.ReplaceMatchingByNiAsync(data, _ct);

        // Assert
        var result = await GetAll<ZhrWsAtribOrgAtribOrg>();
        result.Should().ContainSingle(e => e.Ni == "100" && e.Unid == "Test");
    }

    [Fact]
    public async Task ReplaceMatchingByNiAsync_ShouldHandleLargeDataset()
    {
        // Arrange
        var repository = GetRepository<ZhrWsAptidaoAptidao>();
        var largeList = Enumerable.Range(1, 1000)
            .Select(i => new ZhrWsAptidaoAptidao { Ni = i.ToString(), Subty = "Large" })
            .ToList();

        // Act
        await repository.ReplaceMatchingByNiAsync(largeList, _ct);

        // Assert
        var result = await GetAll<ZhrWsAptidaoAptidao>();
        result.Should().HaveCount(1000);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}

