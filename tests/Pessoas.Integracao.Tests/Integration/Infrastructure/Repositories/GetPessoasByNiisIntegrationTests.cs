using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using Pessoas.Integracao.Core.Domain.Entities;
using Pessoas.Integracao.Core.Infrastructure.Data;
using Pessoas.Integracao.Core.Infrastructure.Repositories;
using Pessoas.Integracao.Tests.TestInfrastructure;

namespace Pessoas.Integracao.Tests.Integration.Infrastructure.Repositories;

[Collection(nameof(PostgresTestDatabaseCollection))]
public sealed class GetPessoasByNiisIntegrationTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly PessoaRepository _repository;
    private readonly CancellationToken _ct = TestContext.Current.CancellationToken;


    public GetPessoasByNiisIntegrationTests(PostgresTestContainerDb db)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseNpgsql(db.ConnectionString)
    .Options;

        _context = new AppDbContext(options);
        _repository = new PessoaRepository(_context);
        _context.Database.EnsureCreated();

    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task ShouldReturnEmptyList_WhenInputEmptyListAndDBIsEmpty()
    {
        // Arrange
        var ct = new CancellationTokenSource().Token;
        var niis = new List<string>();

        // Act
        var result = await _repository.GetPessoasByNiiAsync(niis, ct);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ShouldReturnEmptyList_WhenInputEmptyListAndDBIsPopulated()
    {
        // Arrange
        var pessoas = new[] { new Pessoa { NII = "22600" }, new Pessoa { NII = "22601" } };
        await _context.SaveChangesAsync(_ct);
        await _repository.AddRangeAsync(pessoas, _ct);

        var niis = new List<string>();
        var ct = new CancellationTokenSource().Token;

        // Act
        var result = await _repository.GetPessoasByNiiAsync(niis, ct);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ShouldReturnEmptyList_WhenSingleNiiAndDBIsEmpty()
    {
        // Arrange
        var ct = new CancellationTokenSource().Token;
        var niis = new List<string> { "22600" };

        // Act
        var result = await _repository.GetPessoasByNiiAsync(niis, ct);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ShouldReturnOnePessoa_WhenSingleNiiAndDBPopulatedAndNiiExists()
    {
        // Arrange
        var targetNii = "22600";
        var pessoas = new[] { new Pessoa { NII = targetNii }, new Pessoa { NII = "22601" } };
        await _repository.AddRangeAsync(pessoas, _ct);
        await _context.SaveChangesAsync(_ct);

        var niis = new List<string> { targetNii };
        var ct = new CancellationTokenSource().Token;

        // Act
        var result = await _repository.GetPessoasByNiiAsync(niis, ct);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].NII.Should().Be(targetNii);
    }

    [Fact]
    public async Task ShouldReturnEmptyList_WhenSingleNiiAndDBPopulatedAndNiiDoesNotExist()
    {
        // Arrange
        var pessoas = new[] { new Pessoa { NII = "22600" }, new Pessoa { NII = "22601" } };
        await _repository.AddRangeAsync(pessoas, _ct);
        await _context.SaveChangesAsync(_ct);

        var niis = new List<string> { "99999" };
        var ct = new CancellationTokenSource().Token;

        // Act
        var result = await _repository.GetPessoasByNiiAsync(niis, ct);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ShouldReturnAllMatchingPessoas_WhenAllNiisExistInPopulatedDb()
    {
        // Arrange
        var pessoas = new[] { new Pessoa { NII = "22600" }, new Pessoa { NII = "22601" } };
        await _repository.AddRangeAsync(pessoas, _ct);
        await _context.SaveChangesAsync(_ct);

        var niis = new List<string> { "22600", "22601" };
        var ct = new CancellationTokenSource().Token;

        // Act
        var result = await _repository.GetPessoasByNiiAsync(niis, ct);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Select(p => p.NII).Should().BeEquivalentTo("22600", "22601");
    }

    [Fact]
    public async Task ShouldReturnOnlyMatchingPessoas_WhenSomeNiisExistInPopulatedDb()
    {
        // Arrange
        var pessoas = new[] { new Pessoa { NII = "22600" } };
        await _repository.AddRangeAsync(pessoas, _ct);
        await _context.SaveChangesAsync(_ct);

        var niis = new List<string> { "22600", "22601" };
        var ct = new CancellationTokenSource().Token;

        // Act
        var result = await _repository.GetPessoasByNiiAsync(niis, ct);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].NII.Should().Be("22600");
    }

    [Fact]
    public async Task ShouldReturnEmptyList_WhenNoneNiisExistInPopulatedDb()
    {
        // Arrange
        var pessoas = new[] { new Pessoa { NII = "22600" }, new Pessoa { NII = "22601" } };
        await _repository.AddRangeAsync(pessoas, _ct);
        await _context.SaveChangesAsync(_ct);

        var niis = new List<string> { "22602", "22603" };
        var ct = new CancellationTokenSource().Token;

        // Act
        var result = await _repository.GetPessoasByNiiAsync(niis, ct);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ShouldReturnOnlyMatchingPessoas_WhenInputListHasDuplicatedAndNiiExistInPopulatedDb()
    {
        // Arrange
        var pessoas = new[] { new Pessoa { NII = "22600" } };
        await _repository.AddRangeAsync(pessoas, _ct);
        await _context.SaveChangesAsync(_ct);

        var niis = new List<string> { "22600", "22600", "22601" };
        var ct = new CancellationTokenSource().Token;

        // Act
        var result = await _repository.GetPessoasByNiiAsync(niis, ct);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].NII.Should().Be("22600");
    }

}
