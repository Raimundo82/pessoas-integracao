using System.Net;
using System.Security.Claims;
using System.Text.Json;

using FluentAssertions;

using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;

using Moq;

using Pessoas.Integracao.Core.Application.Contracts;
using Pessoas.Integracao.Core.Application.DTOs;
using Pessoas.Integracao.Core.Application.Security;
using Pessoas.Integracao.Core.Domain.Entities;
using Pessoas.Integracao.Core.Infrastructure.Data;
using Pessoas.Integracao.Tests.TestInfrastructure;

namespace Pessoas.Integracao.Tests.Integration.Controllers;

[Collection(nameof(PostgresTestDatabaseCollection))]
public sealed class PessoasControllerTests : IClassFixture<IntegrationTestWebAppFactory>, IDisposable
{
    private readonly AppDbContext _context;
    private readonly IntegrationTestWebAppFactory _factory;

    public PessoasControllerTests(PostgresTestContainerDb db, IntegrationTestWebAppFactory factory)
    {
        _factory = factory;

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
        using var client = _factory.CreateAuthenticatedClient(Roles.Viewer);
        await _context.Pessoas.AddRangeAsync(
            new Pessoa { NII = "22600", ExternalId = "30002697" },
            new Pessoa { NII = "21200", ExternalId = "30002798" }
        );
        await _context.SaveChangesAsync();

        // Act
        var response = await client.GetAsync("/api/pessoas");

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
        // Arrange
        using var client = _factory.CreateAuthenticatedClient(Roles.Viewer);

        // Act
        var response = await client.GetAsync("/api/pessoas");

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
        using var client = _factory.CreateAuthenticatedClient(Roles.Viewer);
        await _context.Pessoas.AddAsync(new Pessoa { NII = "22600", ExternalId = null });
        await _context.SaveChangesAsync();

        // Act
        var response = await client.GetAsync("/api/pessoas");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var pessoas = await response.Content.ReadFromJsonAsync<List<PessoaDto>>();
        pessoas.Should().ContainSingle();
        pessoas![0].ExternalId.Should().BeNull();
    }

    [Fact]
    public async Task GetAll_AsAdmin_ReturnsOkWithAllPessoaDtos()
    {
        // Arrange
        using var adminClient = _factory.CreateAuthenticatedClient(Roles.Admin);

        await _context.Pessoas.AddRangeAsync(
            new Pessoa { NII = "11111", ExternalId = "TEST001" },
            new Pessoa { NII = "22222", ExternalId = "TEST002" }
        );
        await _context.SaveChangesAsync();

        // Act
        var response = await adminClient.GetAsync("/api/pessoas");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var pessoas = await response.Content.ReadFromJsonAsync<List<PessoaDto>>();
        pessoas.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAll_WhenRepositoryThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        var mockRepo = new Mock<IPessoaRepository>();
        mockRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database failure"));

        using var errorFactory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton(mockRepo.Object);
            });
        });

        var claims = new[]
        {
            new { Type = ClaimTypes.Name, Value = "TestUser" },
            new { Type = ClaimTypes.NameIdentifier, Value = "test-user-id" },
            new { Type = ClaimTypes.Role, Value = Roles.Viewer }
        };
        using var client = errorFactory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Claims", JsonSerializer.Serialize(claims));

        // Act
        var response = await client.GetAsync("/api/pessoas");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task GetAll_Unauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        using var unauthClient = _factory.CreateClient();

        // Act
        var response = await unauthClient.GetAsync("/api/pessoas");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    public void Dispose()
    {
        _context?.Database.EnsureDeleted();
        _context?.Dispose();
        GC.SuppressFinalize(this);
    }
}