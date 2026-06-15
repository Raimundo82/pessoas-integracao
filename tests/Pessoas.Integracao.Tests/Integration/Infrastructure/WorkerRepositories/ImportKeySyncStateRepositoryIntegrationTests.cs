using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using Pessoas.Integracao.Tests.TestInfrastructure;
using Pessoas.Integracao.Worker.Domain.Entities;
using Pessoas.Integracao.Worker.Domain.ValueObjects;
using Pessoas.Integracao.Worker.Infrastructure.Data;
using Pessoas.Integracao.Worker.Infrastructure.Repositories;

namespace Pessoas.Integracao.Tests.Integration.Infrastructure.WorkerRepositories;

[Collection(nameof(PostgresTestDatabaseCollection))]
public sealed class ImportKeySyncStateRepositoryIntegrationTests : IAsyncLifetime, IDisposable
{
    private readonly ImportKeySyncStateDbContext _context;
    private readonly DbContextOptions<ImportKeySyncStateDbContext> _options;
    private readonly ImportKeySyncStateRepository _repository;
    private readonly CancellationToken _ct = TestContext.Current.CancellationToken;
    private readonly PostgresTestContainerDb _db;

    public ImportKeySyncStateRepositoryIntegrationTests(PostgresTestContainerDb db)
    {
        _db = db;

        _options = new DbContextOptionsBuilder<ImportKeySyncStateDbContext>()
            .UseNpgsql(db.ConnectionString)
            .Options;

        _context = new ImportKeySyncStateDbContext(_options);
        _repository = new ImportKeySyncStateRepository(_context);
    }

    public ValueTask InitializeAsync() => new(_db.ResetDatabaseAsync());

    [Fact]
    public async Task ShouldReturnMatchingRows_WhenGetAsyncIsCalled()
    {
        // Arrange
        var existing = new[]
        {
            new ImportKeySyncState { Ni = "10001", Numsap = "A1", SyncState = new SyncState(DateTimeOffset.UtcNow) },
            new ImportKeySyncState { Ni = "20002", Numsap = "A2", SyncState = new SyncState(DateTimeOffset.UtcNow) },
            new ImportKeySyncState { Ni = "30003", Numsap = "A3", SyncState = new SyncState(DateTimeOffset.UtcNow) }
        };

        await _context.ImportKeySyncStates.AddRangeAsync(existing);
        await _context.SaveChangesAsync(_ct);

        var query = new[]
        {
            new ImportKeySyncState { Ni = "10001", Numsap = "X", SyncState = new SyncState(DateTimeOffset.UtcNow) },
            new ImportKeySyncState { Ni = "30003", Numsap = "Y", SyncState = new SyncState(DateTimeOffset.UtcNow) }
        };

        // Act
        var result = await _repository.GetAsync(query, _ct);

        // Assert
        result.Should().HaveCount(2);
        result.Select(e => e.Ni).Should().BeEquivalentTo("10001", "30003");
    }

    [Fact]
    public async Task ShouldReturnEmpty_WhenGetAsyncReceivesEmptyList()
    {
        // Arrange
        await _context.ImportKeySyncStates.AddAsync(
            new ImportKeySyncState { Ni = "99999", Numsap = "X", SyncState = new SyncState(DateTimeOffset.UtcNow) }, _ct
        );
        await _context.SaveChangesAsync(_ct);

        // Act
        var result = await _repository.GetAsync([], _ct);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ShouldInsertRows_WhenUpsertAsyncReceivesNewNis()
    {
        // Arrange
        var items = new[]
        {
            new ImportKeySyncState { Ni = "10001", Numsap = "A1", SyncState = new SyncState(DateTimeOffset.UtcNow) },
            new ImportKeySyncState { Ni = "20002", Numsap = "A2", SyncState = new SyncState(DateTimeOffset.UtcNow) }
        };

        // Act
        await _repository.UpsertAsync(items, _ct);

        // Assert
        var result = await GetAll();
        result.Should().HaveCount(2);
        result.Select(e => e.Ni).Should().BeEquivalentTo("10001", "20002");
    }

    [Fact]
    public async Task ShouldUpdateExistingRows_WhenUpsertAsyncReceivesExistingNis()
    {
        // Arrange
        var oldState = new SyncState(DateTimeOffset.UtcNow.AddDays(-1));

        await _context.ImportKeySyncStates.AddAsync(
            new ImportKeySyncState { Ni = "10001", Numsap = "OLD", SyncState = oldState }, _ct
        );
        await _context.SaveChangesAsync(_ct);

        var newState = new SyncState(DateTimeOffset.UtcNow);

        var items = new[]
        {
            new ImportKeySyncState { Ni = "10001", Numsap = "NEW", SyncState = newState }
        };

        // Act
        await _repository.UpsertAsync(items, _ct);

        // Assert
        var result = await GetAll();
        result.Should().HaveCount(1);

        var row = result.Single();
        row.Numsap.Should().Be("NEW");
        row.SyncState.UpdatedAt.Should().BeAfter(oldState.UpdatedAt);
        row.SyncState.UpdatedAt
            .Should()
            .BeCloseTo(newState.UpdatedAt, TimeSpan.FromMilliseconds(1));
    }

    [Fact]
    public async Task ShouldHandleMixedInsertAndUpdate_WhenUpsertAsyncReceivesMixedNis()
    {
        // Arrange
        await _context.ImportKeySyncStates.AddAsync(
            new ImportKeySyncState { Ni = "20002", Numsap = "OLD", SyncState = new SyncState(DateTimeOffset.UtcNow.AddDays(-1)) }, _ct
        );
        await _context.SaveChangesAsync(_ct);

        var items = new[]
        {
            new ImportKeySyncState { Ni = "20002", Numsap = "UPDATED", SyncState = new SyncState(DateTimeOffset.UtcNow) },
            new ImportKeySyncState { Ni = "30003", Numsap = "NEW", SyncState = new SyncState(DateTimeOffset.UtcNow) }
        };

        // Act
        await _repository.UpsertAsync(items, _ct);

        // Assert
        var result = await GetAll();
        result.Should().HaveCount(2);
        result.Select(e => e.Ni).Should().BeEquivalentTo("20002", "30003");
    }

    [Fact]
    public async Task ShouldDeleteMatchingRows_WhenDeleteAsyncIsCalled()
    {
        // Arrange
        await _context.ImportKeySyncStates.AddRangeAsync(
            new ImportKeySyncState { Ni = "10001", Numsap = "A1", SyncState = new SyncState(DateTimeOffset.UtcNow) },
            new ImportKeySyncState { Ni = "20002", Numsap = "A2", SyncState = new SyncState(DateTimeOffset.UtcNow) },
            new ImportKeySyncState { Ni = "30003", Numsap = "A3", SyncState = new SyncState(DateTimeOffset.UtcNow) }
        );
        await _context.SaveChangesAsync(_ct);

        var toDelete = new[]
        {
            new ImportKeySyncState { Ni = "20002", Numsap = "X", SyncState = new SyncState(DateTimeOffset.UtcNow) },
            new ImportKeySyncState { Ni = "30003", Numsap = "Y", SyncState = new SyncState(DateTimeOffset.UtcNow) }
        };

        // Act
        await _repository.DeleteAsync(toDelete, _ct);

        // Assert
        var result = await GetAll();
        result.Should().HaveCount(1);
        result.Single().Ni.Should().Be("10001");
    }

    [Fact]
    public async Task ShouldReplaceAllRows_WhenReplaceAllAsyncIsCalled()
    {
        // Arrange
        await _context.ImportKeySyncStates.AddRangeAsync(
            new ImportKeySyncState { Ni = "11111", Numsap = "OLD1", SyncState = new SyncState(DateTimeOffset.UtcNow) },
            new ImportKeySyncState { Ni = "22222", Numsap = "OLD2", SyncState = new SyncState(DateTimeOffset.UtcNow) }
        );
        await _context.SaveChangesAsync(_ct);

        var newItems = new[]
        {
            new ImportKeySyncState { Ni = "33333", Numsap = "NEW1", SyncState = new SyncState(DateTimeOffset.UtcNow) },
            new ImportKeySyncState { Ni = "44444", Numsap = "NEW2", SyncState = new SyncState(DateTimeOffset.UtcNow) }
        };

        // Act
        await _repository.ReplaceAllAsync(newItems, _ct);

        // Assert
        var result = await GetAll();
        result.Should().HaveCount(2);
        result.Select(e => e.Ni).Should().BeEquivalentTo("33333", "44444");
    }

    private async Task<List<ImportKeySyncState>> GetAll()
    {
        await using var ctx = new ImportKeySyncStateDbContext(_options);
        return await ctx.ImportKeySyncStates.ToListAsync();
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
