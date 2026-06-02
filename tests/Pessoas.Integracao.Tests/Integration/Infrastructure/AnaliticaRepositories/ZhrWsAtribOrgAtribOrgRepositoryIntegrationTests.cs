using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using Npgsql;

using Pessoas.Integracao.Analitica.Infrastructure.Data;
using Pessoas.Integracao.Analitica.Infrastructure.Repositories;
using Pessoas.Integracao.Analitica.Models;
using Pessoas.Integracao.Tests.TestInfrastructure;

namespace Pessoas.Integracao.Tests.Integration.Infrastructure.AnaliticaRepositories;

[Collection(nameof(PostgresTestDatabaseCollection))]
public sealed class ZhrWsAtribOrgAtribOrgRepositoryIntegrationTests : IAsyncLifetime, IDisposable
{
    private readonly AnaliticaDbContext _context;
    private readonly DbContextOptions<AnaliticaDbContext> _options;
    private readonly ZhrWsAtribOrgAtribOrgRepository _repository;
    private readonly CancellationToken _ct = TestContext.Current.CancellationToken;
    private readonly PostgresTestContainerDb _db;

    public ZhrWsAtribOrgAtribOrgRepositoryIntegrationTests(PostgresTestContainerDb db)
    {
        _db = db;
        _options = new DbContextOptionsBuilder<AnaliticaDbContext>()
            .UseNpgsql(db.ConnectionString)
            .Options;

        _context = new AnaliticaDbContext(_options);
        _repository = new ZhrWsAtribOrgAtribOrgRepository(_context);
    }

    public ValueTask InitializeAsync() => new(_db.ResetDatabaseAsync());

    [Fact]
    public async Task ShouldPreserveAllRows_WhenReplaceMatchingByNiInputIsEmpty()
    {
        // Arrange
        var existing = new[]
        {
            new ZhrWsAtribOrgAtribOrg { Ni = "20002", Unid = "0001", DescUni = "OldUni" },
            new ZhrWsAtribOrgAtribOrg { Ni = "20002", Unid = "0002", DescUni = "OldUni2" },
            new ZhrWsAtribOrgAtribOrg { Ni = "20102", Unid = "0001", DescUni = "OldUni" },
            new ZhrWsAtribOrgAtribOrg { Ni = "20202", Unid = "0001", DescUni = "OldUni" }
        };
        await _context.ZhrWsAtribOrgAtribOrgs.AddRangeAsync(existing);
        await _context.SaveChangesAsync(_ct);

        // Act
        await _repository.ReplaceMatchingByNiAsync([], _ct);

        // Assert
        var result = await GetAllZhrWsAtribOrgAtribOrg();

        result.Should().HaveCount(4);
        result.Should().BeEquivalentTo(existing, options => options.WithoutStrictOrdering());
    }

    [Fact]
    public async Task ShouldInsertAllRows_WhenDatabaseIsEmptyAndNiIsNew()
    {
        // Arrange
        var ni = "10001";
        var units = new[]
        {
            new ZhrWsAtribOrgAtribOrg { Ni = ni, Unid = "0001", DescUni = "DAGI" },
            new ZhrWsAtribOrgAtribOrg { Ni = ni, Unid = "0002", DescUni = "DITIC" }
        };

        // Act
        await _repository.ReplaceMatchingByNiAsync(units, _ct);

        // Assert
        var result = await GetAllZhrWsAtribOrgAtribOrg();

        result.Should().HaveCount(2);
        result.Select(e => e.Unid).Should().BeEquivalentTo("0001", "0002");
    }

    [Fact]
    public async Task ShouldReplaceMatchingByNiRowsAndPreserveUnrelated_WhenNiExists()
    {
        // Arrange
        await _context.ZhrWsAtribOrgAtribOrgs.AddRangeAsync(
            new ZhrWsAtribOrgAtribOrg { Ni = "20002", Unid = "0001", DescUni = "OldUni" },
            new ZhrWsAtribOrgAtribOrg { Ni = "20002", Unid = "0002", DescUni = "OldUni2" },
            new ZhrWsAtribOrgAtribOrg { Ni = "20102", Unid = "0001", DescUni = "OldUni" },
            new ZhrWsAtribOrgAtribOrg { Ni = "20202", Unid = "0001", DescUni = "OldUni" }

        );
        await _context.SaveChangesAsync(_ct);

        var newUnis = new[]
        {
            new ZhrWsAtribOrgAtribOrg { Ni = "20002", Unid = "0003", DescUni = "NewUni" }
        };

        // Act
        await _repository.ReplaceMatchingByNiAsync(newUnis, _ct);

        // Assert
        var result = await GetAllZhrWsAtribOrgAtribOrg();

        result.Should().HaveCount(3);
        result.Where(e => e.Ni == "20002")
            .Should().HaveCount(1).And
            .ContainSingle(e => e.Unid == "0003" && e.DescUni == "NewUni");
    }

    [Fact]
    public async Task ShouldNotAffectOtherNiValues_WhenReplaceMatchingByNiIsCalledForDifferentNi()
    {
        // Arrange
        await _context.ZhrWsAtribOrgAtribOrgs.AddAsync(
            new ZhrWsAtribOrgAtribOrg { Ni = "99999", Unid = "0001", DescUni = "OtherPerson" }, _ct
        );
        await _context.SaveChangesAsync(_ct);

        var units = new[]
        {
            new ZhrWsAtribOrgAtribOrg { Ni = "30003", Unid = "0001", DescUni = "Apto" }
        };

        // Act
        await _repository.ReplaceMatchingByNiAsync(units, _ct);

        // Assert
        var result = await GetAllZhrWsAtribOrgAtribOrg();
        result.Should().HaveCount(2);

        var other = result.Should().ContainSingle(e => e.Ni == "99999").Which;
        other.DescUni.Should().Be("OtherPerson");
    }

    [Fact]
    public async Task ShouldReplaceMatchingByNiRows_WhenMultipleNisProvided()
    {
        // Arrange
        await _context.ZhrWsAtribOrgAtribOrgs.AddRangeAsync(
            new ZhrWsAtribOrgAtribOrg { Ni = "30003", Unid = "0001", DescUni = "OldA" },
            new ZhrWsAtribOrgAtribOrg { Ni = "30003", Unid = "0002", DescUni = "OldB" },
            new ZhrWsAtribOrgAtribOrg { Ni = "40004", Unid = "0001", DescUni = "OldC" }
        );
        await _context.SaveChangesAsync(_ct);

        var units = new[]
        {
            new ZhrWsAtribOrgAtribOrg { Ni = "30003", Unid = "0003", DescUni = "NewA" },
            new ZhrWsAtribOrgAtribOrg { Ni = "40004", Unid = "0004", DescUni = "NewB" },
            new ZhrWsAtribOrgAtribOrg { Ni = "40004", Unid = "0005", DescUni = "NewC" }
        };

        // Act
        await _repository.ReplaceMatchingByNiAsync(units, _ct);

        // Assert
        var result = await GetAllZhrWsAtribOrgAtribOrg();

        var ni30003 = result.Where(e => e.Ni == "30003").ToList();
        ni30003.Should().HaveCount(1);
        ni30003[0].Unid.Should().Be("0003");

        var ni40004 = result.Where(e => e.Ni == "40004").ToList();
        ni40004.Should().HaveCount(2);
        ni40004.Select(e => e.Unid).Should().BeEquivalentTo("0004", "0005");
    }

    [Fact]
    public async Task ShouldRollbackReplaceMatchingByNi_WhenDatabaseErrorOccurs()
    {
        // Arrange
        await _context.ZhrWsAtribOrgAtribOrgs.AddAsync(
            new ZhrWsAtribOrgAtribOrg { Ni = "99999", Unid = "0001", DescUni = "OtherPerson" }, _ct
        );
        await _context.SaveChangesAsync(_ct);

        var newDataset = new[]
        {
            new ZhrWsAtribOrgAtribOrg { Ni = null!, Unid = "0001" },
            new ZhrWsAtribOrgAtribOrg { Ni = "33333", Unid = "0001" },
            new ZhrWsAtribOrgAtribOrg { Ni = "44444", Unid = "0002" }
        };

        // Act
        Func<Task> act = async () => await _repository.ReplaceMatchingByNiAsync(newDataset, _ct);

        await act.Should().ThrowAsync<PostgresException>();

        // Assert
        var result = await GetAllZhrWsAtribOrgAtribOrg();
        result.Should().HaveCount(1)
            .And.ContainSingle(e => e.Ni == "99999" && e.DescUni == "OtherPerson");
    }

    [Fact]
    public async Task ShouldRollbackReplaceMatchingByNi_WhenDatabaseErrorOccursWithEmptyDatabase()
    {
        // Arrange
        var newDataset = new[]
        {
            new ZhrWsAtribOrgAtribOrg { Ni = null!, Unid = "0001" },
            new ZhrWsAtribOrgAtribOrg { Ni = "33333", Unid = "0001" },
            new ZhrWsAtribOrgAtribOrg { Ni = "44444", Unid = "0002" }
        };

        // Act
        Func<Task> act = async () => await _repository.ReplaceMatchingByNiAsync(newDataset, _ct);
        await act.Should().ThrowAsync<PostgresException>();

        // Assert
        var result = await GetAllZhrWsAtribOrgAtribOrg();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ShouldLeaveDatabaseEmpty_WhenReplaceAllInputIsEmpty()
    {
        // Arrange
        var existing = new[]
        {
            new ZhrWsAtribOrgAtribOrg { Ni = "20002", Unid = "0001", DescUni = "OldUni" },
            new ZhrWsAtribOrgAtribOrg { Ni = "20002", Unid = "0002", DescUni = "OldUni2" },
            new ZhrWsAtribOrgAtribOrg { Ni = "20102", Unid = "0001", DescUni = "OldUni" },
            new ZhrWsAtribOrgAtribOrg { Ni = "20202", Unid = "0001", DescUni = "OldUni" }
        };
        await _context.ZhrWsAtribOrgAtribOrgs.AddRangeAsync(existing);
        await _context.SaveChangesAsync(_ct);

        // Act
        await _repository.ReplaceAllAsync([], _ct);

        // Assert
        var result = await GetAllZhrWsAtribOrgAtribOrg();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ShouldContainOnlyNewRecords_WhenReplaceAllAsyncIsCalledWithPopulatedDatabase()
    {
        // Arrange
        await _context.ZhrWsAtribOrgAtribOrgs.AddRangeAsync(
            new ZhrWsAtribOrgAtribOrg { Ni = "11111", Unid = "0001" },
            new ZhrWsAtribOrgAtribOrg { Ni = "22222", Unid = "0001" }
        );
        await _context.SaveChangesAsync(_ct);

        var newDataset = new[]
        {
            new ZhrWsAtribOrgAtribOrg { Ni = "33333", Unid = "0001" },
            new ZhrWsAtribOrgAtribOrg { Ni = "44444", Unid = "0002" }
        };

        // Act
        await _repository.ReplaceAllAsync(newDataset, _ct);

        // Assert
        var result = await GetAllZhrWsAtribOrgAtribOrg();

        result.Should().HaveCount(2);
        result.Select(e => e.Ni).Should().BeEquivalentTo("33333", "44444");
    }

    [Fact]
    public async Task ShouldContainOnlyNewRecords_WhenReplaceAllAsyncIsCalledWithEmptyDatabase()
    {
        // Arrange
        var newDataset = new[]
        {
            new ZhrWsAtribOrgAtribOrg { Ni = "33333", Unid = "0001" },
            new ZhrWsAtribOrgAtribOrg { Ni = "44444", Unid = "0002" }
        };

        // Act
        await _repository.ReplaceAllAsync(newDataset, _ct);

        // Assert
        var result = await GetAllZhrWsAtribOrgAtribOrg();

        result.Should().HaveCount(2);
        result.Select(e => e.Ni).Should().BeEquivalentTo("33333", "44444");
    }

    [Fact]
    public async Task ShouldRollbackReplaceAll_WhenDatabaseErrorOccurs()
    {
        // Arrange
        await _context.ZhrWsAtribOrgAtribOrgs.AddAsync(
            new ZhrWsAtribOrgAtribOrg { Ni = "99999", Unid = "0001", DescUni = "OtherPerson" }, _ct
        );
        await _context.SaveChangesAsync(_ct);

        var newDataset = new[]
        {
            new ZhrWsAtribOrgAtribOrg { Ni = null!, Unid = "0001" },
            new ZhrWsAtribOrgAtribOrg { Ni = "33333", Unid = "0001" },
            new ZhrWsAtribOrgAtribOrg { Ni = "44444", Unid = "0002" }
        };

        // Act
        Func<Task> act = async () => await _repository.ReplaceAllAsync(newDataset, _ct);

        await act.Should().ThrowAsync<PostgresException>();

        // Assert
        var result = await GetAllZhrWsAtribOrgAtribOrg();
        result.Should().HaveCount(1)
            .And.ContainSingle(e => e.Ni == "99999" && e.DescUni == "OtherPerson");
    }

    [Fact]
    public async Task ShouldRollbackReplaceAll_WhenDatabaseErrorOccursWithEmptyDatabase()
    {
        // Arrange
        var newDataset = new[]
        {
            new ZhrWsAtribOrgAtribOrg { Ni = null!, Unid = "0001" },
            new ZhrWsAtribOrgAtribOrg { Ni = "33333", Unid = "0001" },
            new ZhrWsAtribOrgAtribOrg { Ni = "44444", Unid = "0002" }
        };

        // Act
        Func<Task> act = async () => await _repository.ReplaceAllAsync(newDataset, _ct);
        await act.Should().ThrowAsync<PostgresException>();

        // Assert
        var result = await GetAllZhrWsAtribOrgAtribOrg();
        result.Should().BeEmpty();
    }

    private async Task<List<ZhrWsAtribOrgAtribOrg>> GetAllZhrWsAtribOrgAtribOrg()
    {
        await using var context = new AnaliticaDbContext(_options);
        return await context.ZhrWsAtribOrgAtribOrgs.ToListAsync();
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
