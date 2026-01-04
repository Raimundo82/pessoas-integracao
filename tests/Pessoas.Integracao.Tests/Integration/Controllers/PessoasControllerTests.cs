using System.Net;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using Pessoas.Integracao.Core.Application.DTOs;
using Pessoas.Integracao.Core.Domain.Entities;
using Pessoas.Integracao.Core.Infrastructure.Data;
using Pessoas.Integracao.Tests.TestInfrastructure;

namespace Pessoas.Integracao.Tests.Integration.Controllers;

[Collection(nameof(PostgresTestDatabaseCollection))]
public sealed class PessoasControllerTests : IClassFixture<IntegrationTestWebAppFactory>, IDisposable
{
    private readonly HttpClient _client;
    private readonly AppDbContext _context;

    public PessoasControllerTests(PostgresTestContainerDb db, IntegrationTestWebAppFactory factory)
    {
        _client = factory.CreateClient();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(db.ConnectionString)
            .Options;

        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();
    }

    [Fact]
    public async Task GetAll_WhenPessoasExist_ReturnsOkWithAllPessoaDtos()
    {
        // Arrange
        await _context.Pessoas.AddRangeAsync(
            new Pessoa { NII = "22600", ExternalId = "30002697" },
            new Pessoa { NII = "21200", ExternalId = "30002798" }
        );
        await _context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/pessoas");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var pessoas = await response.Content.ReadFromJsonAsync<List<PessoaDto>>();
        pessoas.Should().NotBeNull();
        pessoas.Should().HaveCount(2);
        pessoas.Should().ContainEquivalentOf(new PessoaDto("22600", "30002697"));
        pessoas.Should().ContainEquivalentOf(new PessoaDto("21200", "30002798"));
    }

    [Fact]
    public async Task GetAll_WhenNoPessoasExist_ReturnsOkWithEmptyArray()
    {
        // Act
        var response = await _client.GetAsync("/api/pessoas");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var pessoas = await response.Content.ReadFromJsonAsync<List<PessoaDto>>();
        pessoas.Should().NotBeNull();
        pessoas.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAll_WithNullExternalId_ReturnsCorrectly()
    {
        // Arrange
        await _context.Pessoas.AddAsync(new Pessoa { NII = "22600", ExternalId = null });
        await _context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/pessoas");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var pessoas = await response.Content.ReadFromJsonAsync<List<PessoaDto>>();
        pessoas.Should().ContainSingle();
        pessoas![0].ExternalId.Should().BeNull();
    }

    public void Dispose()
    {
        _context?.Database.EnsureDeleted();
        _context?.Dispose();
        GC.SuppressFinalize(this);
    }
}