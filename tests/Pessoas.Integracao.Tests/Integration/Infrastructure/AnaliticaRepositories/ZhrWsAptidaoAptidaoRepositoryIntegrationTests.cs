using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using Npgsql;

using Pessoas.Integracao.Analitica.Infrastructure.Data;
using Pessoas.Integracao.Analitica.Infrastructure.Repositories;
using Pessoas.Integracao.Analitica.Models;
using Pessoas.Integracao.Tests.TestInfrastructure;

namespace Pessoas.Integracao.Tests.Integration.Infrastructure.AnaliticaRepositories;

[Collection(nameof(PostgresTestDatabaseCollection))]
public sealed class ZhrWsAptidaoAptidaoRepositoryIntegrationTests : IAsyncLifetime, IDisposable
{
    private readonly AnaliticaDbContext _context;
    private readonly DbContextOptions<AnaliticaDbContext> _options;
    private readonly ZhrWsAptidaoAptidaoRepository _repository;
    private readonly CancellationToken _ct = TestContext.Current.CancellationToken;
    private readonly PostgresTestContainerDb _db;

    public ZhrWsAptidaoAptidaoRepositoryIntegrationTests(PostgresTestContainerDb db)
    {
        _db = db;
        _options = new DbContextOptionsBuilder<AnaliticaDbContext>()
            .UseNpgsql(db.ConnectionString)
            .Options;

        _context = new AnaliticaDbContext(_options);
        _repository = new ZhrWsAptidaoAptidaoRepository(_context);
    }

    public ValueTask InitializeAsync() => new(_db.ResetDatabaseAsync());

    [Fact]
    public async Task ShouldPreserveAllRows_WhenReplaceMatchingByNiInputIsEmpty()
    {
        // Arrange
        var existing = new[]
        {
            new ZhrWsAptidaoAptidao { Ni = "20002", Subty = "0001", Denominacao = "OldExam" },
            new ZhrWsAptidaoAptidao { Ni = "20002", Subty = "0002", Denominacao = "OldExam2" },
            new ZhrWsAptidaoAptidao { Ni = "20102", Subty = "0001", Denominacao = "OldExam" },
            new ZhrWsAptidaoAptidao { Ni = "20202", Subty = "0001", Denominacao = "OldExam" }
        };
        await _context.ZhrWsAptidaoAptidaos.AddRangeAsync(existing);
        await _context.SaveChangesAsync(_ct);

        // Act
        await _repository.ReplaceMatchingByNiAsync([], _ct);

        // Assert
        var result = await GetAllZhrWsAptidaoAptidao();

        result.Should().HaveCount(4);
        result.Should().BeEquivalentTo(existing, options => options.WithoutStrictOrdering());
    }

    [Fact]
    public async Task ShouldInsertAllRows_WhenDatabaseIsEmptyAndNiIsNew()
    {
        // Arrange
        var ni = "10001";
        var exams = new[]
        {
            new ZhrWsAptidaoAptidao { Ni = ni, Subty = "0001", Denominacao = "Apto" },
            new ZhrWsAptidaoAptidao { Ni = ni, Subty = "0002", Denominacao = "Raio-X" }
        };

        // Act
        await _repository.ReplaceMatchingByNiAsync(exams, _ct);

        // Assert
        var result = await GetAllZhrWsAptidaoAptidao();

        result.Should().HaveCount(2);
        result.Select(e => e.Subty).Should().BeEquivalentTo("0001", "0002");
    }

    [Fact]
    public async Task ShouldReplaceMatchingByNiRowsAndPreserveUnrelated_WhenNiExists()
    {
        // Arrange
        await _context.ZhrWsAptidaoAptidaos.AddRangeAsync(
            new ZhrWsAptidaoAptidao { Ni = "20002", Subty = "0001", Denominacao = "OldExam" },
            new ZhrWsAptidaoAptidao { Ni = "20002", Subty = "0002", Denominacao = "OldExam2" },
            new ZhrWsAptidaoAptidao { Ni = "20102", Subty = "0001", Denominacao = "OldExam" },
            new ZhrWsAptidaoAptidao { Ni = "20202", Subty = "0001", Denominacao = "OldExam" }

        );
        await _context.SaveChangesAsync(_ct);

        var newExams = new[]
        {
            new ZhrWsAptidaoAptidao { Ni = "20002", Subty = "0003", Denominacao = "NewExam" }
        };

        // Act
        await _repository.ReplaceMatchingByNiAsync(newExams, _ct);

        // Assert
        var result = await GetAllZhrWsAptidaoAptidao();

        result.Should().HaveCount(3);
        result.Where(e => e.Ni == "20002")
            .Should().HaveCount(1).And
            .ContainSingle(e => e.Subty == "0003" && e.Denominacao == "NewExam");
    }

    [Fact]
    public async Task ShouldNotAffectOtherNiValues_WhenReplaceMatchingByNiIsCalledForDifferentNi()
    {
        // Arrange
        await _context.ZhrWsAptidaoAptidaos.AddAsync(
            new ZhrWsAptidaoAptidao { Ni = "99999", Subty = "0001", Denominacao = "OtherPerson" }, _ct
        );
        await _context.SaveChangesAsync(_ct);

        var exams = new[]
        {
            new ZhrWsAptidaoAptidao { Ni = "30003", Subty = "0001", Denominacao = "Apto" }
        };

        // Act
        await _repository.ReplaceMatchingByNiAsync(exams, _ct);

        // Assert
        var result = await GetAllZhrWsAptidaoAptidao();
        result.Should().HaveCount(2);

        var other = result.Should().ContainSingle(e => e.Ni == "99999").Which;
        other.Denominacao.Should().Be("OtherPerson");
    }

    [Fact]
    public async Task ShouldReplaceMatchingByNiRows_WhenMultipleNisProvided()
    {
        // Arrange
        await _context.ZhrWsAptidaoAptidaos.AddRangeAsync(
            new ZhrWsAptidaoAptidao { Ni = "30003", Subty = "0001", Denominacao = "OldA" },
            new ZhrWsAptidaoAptidao { Ni = "30003", Subty = "0002", Denominacao = "OldB" },
            new ZhrWsAptidaoAptidao { Ni = "40004", Subty = "0001", Denominacao = "OldC" }
        );
        await _context.SaveChangesAsync(_ct);

        var exams = new[]
        {
            new ZhrWsAptidaoAptidao { Ni = "30003", Subty = "0003", Denominacao = "NewA" },
            new ZhrWsAptidaoAptidao { Ni = "40004", Subty = "0004", Denominacao = "NewB" },
            new ZhrWsAptidaoAptidao { Ni = "40004", Subty = "0005", Denominacao = "NewC" }
        };

        // Act
        await _repository.ReplaceMatchingByNiAsync(exams, _ct);

        // Assert
        var result = await GetAllZhrWsAptidaoAptidao();

        var ni30003 = result.Where(e => e.Ni == "30003").ToList();
        ni30003.Should().HaveCount(1);
        ni30003[0].Subty.Should().Be("0003");

        var ni40004 = result.Where(e => e.Ni == "40004").ToList();
        ni40004.Should().HaveCount(2);
        ni40004.Select(e => e.Subty).Should().BeEquivalentTo("0004", "0005");
    }

    [Fact]
    public async Task ShouldRollbackReplaceMatchingByNi_WhenDatabaseErrorOccurs()
    {
        // Arrange
        await _context.ZhrWsAptidaoAptidaos.AddAsync(
            new ZhrWsAptidaoAptidao { Ni = "99999", Subty = "0001", Denominacao = "OtherPerson" }, _ct
        );
        await _context.SaveChangesAsync(_ct);

        var newDataset = new[]
        {
            new ZhrWsAptidaoAptidao { Ni = null!, Subty = "0001" },
            new ZhrWsAptidaoAptidao { Ni = "33333", Subty = "0001" },
            new ZhrWsAptidaoAptidao { Ni = "44444", Subty = "0002" }
        };

        // Act
        Func<Task> act = async () => await _repository.ReplaceMatchingByNiAsync(newDataset, _ct);

        await act.Should().ThrowAsync<PostgresException>();

        // Assert
        var result = await GetAllZhrWsAptidaoAptidao();
        result.Should().HaveCount(1)
            .And.ContainSingle(e => e.Ni == "99999" && e.Denominacao == "OtherPerson");
    }

    [Fact]
    public async Task ShouldRollbackReplaceMatchingByNi_WhenDatabaseErrorOccursWithEmptyDatabase()
    {
        // Arrange
        var newDataset = new[]
        {
            new ZhrWsAptidaoAptidao { Ni = null!, Subty = "0001" },
            new ZhrWsAptidaoAptidao { Ni = "33333", Subty = "0001" },
            new ZhrWsAptidaoAptidao { Ni = "44444", Subty = "0002" }
        };

        // Act
        Func<Task> act = async () => await _repository.ReplaceMatchingByNiAsync(newDataset, _ct);
        await act.Should().ThrowAsync<PostgresException>();

        // Assert
        var result = await GetAllZhrWsAptidaoAptidao();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ShouldLeaveDatabaseEmpty_WhenReplaceAllInputIsEmpty()
    {
        // Arrange
        var existing = new[]
        {
            new ZhrWsAptidaoAptidao { Ni = "20002", Subty = "0001", Denominacao = "OldExam" },
            new ZhrWsAptidaoAptidao { Ni = "20002", Subty = "0002", Denominacao = "OldExam2" },
            new ZhrWsAptidaoAptidao { Ni = "20102", Subty = "0001", Denominacao = "OldExam" },
            new ZhrWsAptidaoAptidao { Ni = "20202", Subty = "0001", Denominacao = "OldExam" }
        };
        await _context.ZhrWsAptidaoAptidaos.AddRangeAsync(existing);
        await _context.SaveChangesAsync(_ct);

        // Act
        await _repository.ReplaceAllAsync([], _ct);

        // Assert
        var result = await GetAllZhrWsAptidaoAptidao();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ShouldContainOnlyNewRecords_WhenReplaceAllAsyncIsCalledWithPopulatedDatabase()
    {
        // Arrange
        await _context.ZhrWsAptidaoAptidaos.AddRangeAsync(
            new ZhrWsAptidaoAptidao { Ni = "11111", Subty = "0001" },
            new ZhrWsAptidaoAptidao { Ni = "22222", Subty = "0001" }
        );
        await _context.SaveChangesAsync(_ct);

        var newDataset = new[]
        {
            new ZhrWsAptidaoAptidao { Ni = "33333", Subty = "0001" },
            new ZhrWsAptidaoAptidao { Ni = "44444", Subty = "0002" }
        };

        // Act
        await _repository.ReplaceAllAsync(newDataset, _ct);

        // Assert
        var result = await GetAllZhrWsAptidaoAptidao();

        result.Should().HaveCount(2);
        result.Select(e => e.Ni).Should().BeEquivalentTo("33333", "44444");
    }

    [Fact]
    public async Task ShouldContainOnlyNewRecords_WhenReplaceAllAsyncIsCalledWithEmptyDatabase()
    {
        // Arrange
        var newDataset = new[]
        {
            new ZhrWsAptidaoAptidao { Ni = "33333", Subty = "0001" },
            new ZhrWsAptidaoAptidao { Ni = "44444", Subty = "0002" }
        };

        // Act
        await _repository.ReplaceAllAsync(newDataset, _ct);

        // Assert
        var result = await GetAllZhrWsAptidaoAptidao();

        result.Should().HaveCount(2);
        result.Select(e => e.Ni).Should().BeEquivalentTo("33333", "44444");
    }

    [Fact]
    public async Task ShouldRollbackReplaceAll_WhenDatabaseErrorOccurs()
    {
        // Arrange
        await _context.ZhrWsAptidaoAptidaos.AddAsync(
            new ZhrWsAptidaoAptidao { Ni = "99999", Subty = "0001", Denominacao = "OtherPerson" }, _ct
        );
        await _context.SaveChangesAsync(_ct);

        var newDataset = new[]
        {
            new ZhrWsAptidaoAptidao { Ni = null!, Subty = "0001" },
            new ZhrWsAptidaoAptidao { Ni = "33333", Subty = "0001" },
            new ZhrWsAptidaoAptidao { Ni = "44444", Subty = "0002" }
        };

        // Act
        Func<Task> act = async () => await _repository.ReplaceAllAsync(newDataset, _ct);

        await act.Should().ThrowAsync<PostgresException>();

        // Assert
        var result = await GetAllZhrWsAptidaoAptidao();
        result.Should().HaveCount(1)
            .And.ContainSingle(e => e.Ni == "99999" && e.Denominacao == "OtherPerson");
    }

    [Fact]
    public async Task ShouldRollbackReplaceAll_WhenDatabaseErrorOccursWithEmptyDatabase()
    {
        // Arrange
        var newDataset = new[]
        {
            new ZhrWsAptidaoAptidao { Ni = null!, Subty = "0001" },
            new ZhrWsAptidaoAptidao { Ni = "33333", Subty = "0001" },
            new ZhrWsAptidaoAptidao { Ni = "44444", Subty = "0002" }
        };

        // Act
        Func<Task> act = async () => await _repository.ReplaceAllAsync(newDataset, _ct);
        await act.Should().ThrowAsync<PostgresException>();

        // Assert
        var result = await GetAllZhrWsAptidaoAptidao();
        result.Should().BeEmpty();
    }

    private async Task<List<ZhrWsAptidaoAptidao>> GetAllZhrWsAptidaoAptidao()
    {
        await using var context = new AnaliticaDbContext(_options);
        return await context.ZhrWsAptidaoAptidaos.ToListAsync();
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
