using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using Pessoas.Integracao.Sync.Domain.Entities;
using Pessoas.Integracao.Sync.Domain.ValueObjects;
using Pessoas.Integracao.Sync.Infrastructure.Data;
using Pessoas.Integracao.Sync.Infrastructure.Repositories;
using Pessoas.Integracao.Testing;

namespace Pessoas.Integracao.Sync.Tests.Integration.Infrastructure.SyncRepositories;

[Collection(nameof(PostgresTestDatabaseCollection))]
public sealed class PessoaSyncRefRepositoryIntegrationTests : IAsyncLifetime, IDisposable
{
    private readonly PessoaSyncRefDbContext _context;
    private readonly DbContextOptions<PessoaSyncRefDbContext> _options;
    private readonly PessoaSyncRefRepository _repository;
    private readonly CancellationToken _ct = TestContext.Current.CancellationToken;
    private readonly PostgresTestContainerDb _db;

    public PessoaSyncRefRepositoryIntegrationTests(PostgresTestContainerDb db)
    {
        _db = db;

        _options = new DbContextOptionsBuilder<PessoaSyncRefDbContext>()
            .UseNpgsql(db.ConnectionString)
            .Options;

        _context = new PessoaSyncRefDbContext(_options);
        _repository = new PessoaSyncRefRepository(_context);
    }

    public ValueTask InitializeAsync() => new(_db.ResetDatabaseAsync());

    [Fact]
    public async Task ShouldReturnMatchingRows_WhenGetByNiAsyncIsCalled()
    {
        // Arrange
        var existing = new[]
        {
            new PessoaSyncRef { Ni = "10001", ExternalId = "A1", SyncState = new SyncState(DateTimeOffset.UtcNow) },
            new PessoaSyncRef { Ni = "20002", ExternalId = "A2", SyncState = new SyncState(DateTimeOffset.UtcNow) },
            new PessoaSyncRef { Ni = "30003", ExternalId = "A3", SyncState = new SyncState(DateTimeOffset.UtcNow) }
        };

        await _context.PessoaSyncRefs.AddRangeAsync(existing);
        await _context.SaveChangesAsync(_ct);

        var niList = new[] { "10001", "30003" };

        // Act
        var result = await _repository.GetByNiAsync(niList, _ct);

        // Assert
        result.Should().HaveCount(2);
        result.Select(e => e.Ni).Should().BeEquivalentTo("10001", "30003");
    }


    [Fact]
    public async Task ShouldReturnEmpty_WhenGetByNiAsyncReceivesEmptyList()
    {
        // Arrange
        await _context.PessoaSyncRefs.AddAsync(
            new PessoaSyncRef { Ni = "99999", ExternalId = "X", SyncState = new SyncState(DateTimeOffset.UtcNow) }, _ct
        );
        await _context.SaveChangesAsync(_ct);

        // Act
        var result = await _repository.GetByNiAsync([], _ct);

        // Assert
        result.Should().BeEmpty();
    }


    [Fact]
    public async Task ShouldInsertRows_WhenUpsertAsyncReceivesNewNis()
    {
        // Arrange
        var items = new[]
        {
            new PessoaSyncRef { Ni = "10001", ExternalId = "A1", SyncState = new SyncState(DateTimeOffset.UtcNow) },
            new PessoaSyncRef { Ni = "20002", ExternalId = "A2", SyncState = new SyncState(DateTimeOffset.UtcNow) }
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

        await _context.PessoaSyncRefs.AddAsync(
            new PessoaSyncRef { Ni = "10001", ExternalId = "OLD", SyncState = oldState }, _ct
        );
        await _context.SaveChangesAsync(_ct);

        var newState = new SyncState(DateTimeOffset.UtcNow);

        var items = new[]
        {
            new PessoaSyncRef { Ni = "10001", ExternalId = "NEW", SyncState = newState }
        };

        // Act
        await _repository.UpsertAsync(items, _ct);

        // Assert
        var result = await GetAll();
        result.Should().HaveCount(1);

        var row = result.Single();
        row.ExternalId.Should().Be("NEW");
        row.SyncState.UpdatedAt.Should().BeAfter(oldState.UpdatedAt);
        row.SyncState.UpdatedAt
            .Should()
            .BeCloseTo(newState.UpdatedAt, TimeSpan.FromMilliseconds(1));
    }

    [Fact]
    public async Task ShouldHandleMixedInsertAndUpdate_WhenUpsertAsyncReceivesMixedNis()
    {
        // Arrange
        await _context.PessoaSyncRefs.AddAsync(
            new PessoaSyncRef { Ni = "20002", ExternalId = "OLD", SyncState = new SyncState(DateTimeOffset.UtcNow.AddDays(-1)) }, _ct
        );
        await _context.SaveChangesAsync(_ct);

        var items = new[]
        {
            new PessoaSyncRef { Ni = "20002", ExternalId = "UPDATED", SyncState = new SyncState(DateTimeOffset.UtcNow) },
            new PessoaSyncRef { Ni = "30003", ExternalId = "NEW", SyncState = new SyncState(DateTimeOffset.UtcNow) }
        };

        // Act
        await _repository.UpsertAsync(items, _ct);

        // Assert
        var result = await GetAll();
        result.Should().HaveCount(2);
        result.Select(e => e.Ni).Should().BeEquivalentTo("20002", "30003");
    }

    [Fact]
    public async Task ShouldDeleteMatchingRows_WhenDeleteByNiAsyncIsCalled()
    {
        // Arrange
        await _context.PessoaSyncRefs.AddRangeAsync(
            new PessoaSyncRef { Ni = "10001", ExternalId = "A1", SyncState = new SyncState(DateTimeOffset.UtcNow) },
            new PessoaSyncRef { Ni = "20002", ExternalId = "A2", SyncState = new SyncState(DateTimeOffset.UtcNow) },
            new PessoaSyncRef { Ni = "30003", ExternalId = "A3", SyncState = new SyncState(DateTimeOffset.UtcNow) }
        );
        await _context.SaveChangesAsync(_ct);

        var niList = new[] { "20002", "30003" };

        // Act
        await _repository.DeleteByNiAsync(niList, _ct);

        // Assert
        var result = await GetAll();
        result.Should().HaveCount(1);
        result.Single().Ni.Should().Be("10001");
    }

    [Fact]
    public async Task ShouldReplaceAllRows_WhenReplaceAllAsyncIsCalled()
    {
        // Arrange
        await _context.PessoaSyncRefs.AddRangeAsync(
            new PessoaSyncRef { Ni = "11111", ExternalId = "OLD1", SyncState = new SyncState(DateTimeOffset.UtcNow) },
            new PessoaSyncRef { Ni = "22222", ExternalId = "OLD2", SyncState = new SyncState(DateTimeOffset.UtcNow) }
        );
        await _context.SaveChangesAsync(_ct);

        var newItems = new[]
        {
            new PessoaSyncRef { Ni = "33333", ExternalId = "NEW1", SyncState = new SyncState(DateTimeOffset.UtcNow) },
            new PessoaSyncRef { Ni = "44444", ExternalId = "NEW2", SyncState = new SyncState(DateTimeOffset.UtcNow) }
        };

        // Act
        await _repository.ReplaceAllAsync(newItems, _ct);

        // Assert
        var result = await GetAll();
        result.Should().HaveCount(2);
        result.Select(e => e.Ni).Should().BeEquivalentTo("33333", "44444");
    }

    private async Task<List<PessoaSyncRef>> GetAll()
    {
        await using var ctx = new PessoaSyncRefDbContext(_options);
        return await ctx.PessoaSyncRefs.ToListAsync();
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
