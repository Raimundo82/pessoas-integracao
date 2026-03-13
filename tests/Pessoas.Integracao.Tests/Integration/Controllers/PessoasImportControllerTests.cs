using System.Net;

using FluentAssertions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Moq;

using Pessoas.Integracao.Core.Application.Contracts;
using Pessoas.Integracao.Core.Application.DTOs;
using Pessoas.Integracao.Core.Application.Models;
using Pessoas.Integracao.Core.Application.Security;
using Pessoas.Integracao.Core.Domain.Entities;
using Pessoas.Integracao.Core.Infrastructure.Data;
using Pessoas.Integracao.Tests.TestInfrastructure;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Channel;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Generated.Output;

namespace Pessoas.Integracao.Tests.Integration.Controllers;

[Collection(nameof(PostgresTestDatabaseCollection))]
public sealed class PessoasImportControllerTests : IClassFixture<IntegrationTestWebAppFactory>, IDisposable
{
    private readonly HttpClient _client;
    private readonly AppDbContext _context;
    private readonly IServiceScope _scope;
    private readonly Mock<zhr_wsChannel> _mockSoapChannel;

    private readonly Mock<IPessoasDataProvider> _mockPessoasDataProvider;
    private readonly IntegrationTestWebAppFactory _factory;

    public PessoasImportControllerTests(PostgresTestContainerDb db, IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateAuthenticatedClient(Roles.Admin);
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(db.ConnectionString)
            .Options;

        _context = new AppDbContext(options);
        _scope = factory.Services.CreateScope();

        _mockSoapChannel = new Mock<zhr_wsChannel>();
        var mockSoapChannelProvider = _scope.ServiceProvider.GetRequiredService<Mock<ISoapChannelProvider<zhr_wsChannel>>>();
        _mockPessoasDataProvider = _scope.ServiceProvider.GetRequiredService<Mock<IPessoasDataProvider>>();


        mockSoapChannelProvider.Setup(f => f.CreateChannel())
            .Returns(_mockSoapChannel.Object);

        _mockPessoasDataProvider
            .Setup(p => p.GetPessoasByImportKeysAsync(It.IsAny<IReadOnlyList<PessoaImportKey>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyList<PessoaImportKey> keys, CancellationToken _) =>
                [.. keys.Select(k => new Pessoa
                {
                    NII = k.Nii,
                    ExternalId = k.ExternalId
                })]);

        _context.Database.EnsureCreated();
    }

    [Fact]
    public async Task ShouldPersistProviderPessoasToDatabase_WhenImportWithMockedSoapResponseAndEmptyDb()
    {
        // Arrange
        var soapResponse = new[]
        {
            new ZhrSListapessoal { Ni = "22600", Numsap = "30002697", Empresa = "3000" },
            new ZhrSListapessoal { Ni = "21200", Numsap = "30002798", Empresa = "3000" }
        };
        _mockSoapChannel
            .Setup(c => c.ZhrWsGetPernrAsync(It.IsAny<ZhrWsGetPernrRequest>()))
            .ReturnsAsync(new ZhrWsGetPernrResponse1
            {
                ZhrWsGetPernrResponse = new ZhrWsGetPernrResponse
                {
                    Output = [new ZhrSGetListapessoal { Pessoal = soapResponse }]
                }
            });


        // Act
        var response = await _client.PostAsync("/api/pessoas/import", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);

        var dto = await response.Content.ReadFromJsonAsync<ImportPessoasResultDto>();
        dto.Should().NotBeNull();
        dto.TotalProcessed.Should().Be(2);
        dto.TotalAdded.Should().Be(2);
        dto.TotalUpdated.Should().Be(0);

        var savedPessoas = await _context.Pessoas.AsNoTracking().ToListAsync();
        savedPessoas.Should().HaveCount(2);
        savedPessoas.Select(p => p.NII).Should().BeEquivalentTo("22600", "21200");
        savedPessoas.Select(p => p.ExternalId).Should().BeEquivalentTo("30002697", "30002798");
    }

    [Fact]
    public async Task ShouldPreserveAllPessoasFromDatabase_WhenImportWithEmptySoapResponse()
    {
        // Arrange
        var existingPessoas = new[]
        {
            new Pessoa {NII = "11111", ExternalId = "OLD001"},
            new Pessoa {NII = "22222", ExternalId = "OLD002"}
        };
        await _context.Pessoas.AddRangeAsync(existingPessoas);
        await _context.SaveChangesAsync();

        _mockSoapChannel
            .Setup(c => c.ZhrWsGetPernrAsync(It.IsAny<ZhrWsGetPernrRequest>()))
            .ReturnsAsync(new ZhrWsGetPernrResponse1
            {
                ZhrWsGetPernrResponse = new ZhrWsGetPernrResponse
                {
                    Output = [new ZhrSGetListapessoal { Pessoal = [] }]
                }
            });

        // Act
        var response = await _client.PostAsync("/api/pessoas/import", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);

        var dto = await response.Content.ReadFromJsonAsync<ImportPessoasResultDto>();
        dto.Should().NotBeNull();
        dto.TotalProcessed.Should().Be(2);
        dto.TotalAdded.Should().Be(0);
        dto.TotalUpdated.Should().Be(2);

        var savedPessoas = await _context.Pessoas.AsNoTracking().ToListAsync();
        savedPessoas.Should().HaveCount(2);
        savedPessoas.Select(p => p.NII).Should().BeEquivalentTo("11111", "22222");
        savedPessoas.Select(p => p.ExternalId).Should().BeEquivalentTo("OLD001", "OLD002");
    }

    [Fact]
    public async Task ShouldKeepUntouchedData_WhenImportUpdatesExistingAndAddsNew()
    {
        // Arrange 
        var existingPessoas = new[]
        {
            new Pessoa {NII = "11111", ExternalId = "OLD001"},
            new Pessoa {NII = "22222", ExternalId = "OLD002"}
        };
        await _context.Pessoas.AddRangeAsync(existingPessoas);
        await _context.SaveChangesAsync();

        var newSoapResponse = new[]
        {
            new ZhrSListapessoal { Ni = "11111", Numsap = "NEW001", Empresa = "3000" },
            new ZhrSListapessoal { Ni = "33333", Numsap = "NEW002", Empresa = "3000" }
        };
        _mockSoapChannel
            .Setup(c => c.ZhrWsGetPernrAsync(It.IsAny<ZhrWsGetPernrRequest>()))
            .ReturnsAsync(new ZhrWsGetPernrResponse1
            {
                ZhrWsGetPernrResponse = new ZhrWsGetPernrResponse
                {
                    Output = [new ZhrSGetListapessoal { Pessoal = newSoapResponse }]
                }
            });

        // Act
        var response = await _client.PostAsync("/api/pessoas/import", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);

        var dto = await response.Content.ReadFromJsonAsync<ImportPessoasResultDto>();
        dto.Should().NotBeNull();
        dto.TotalProcessed.Should().Be(3);
        dto.TotalAdded.Should().Be(1);
        dto.TotalUpdated.Should().Be(2);

        var savedPessoas = await _context.Pessoas.AsNoTracking().ToListAsync();
        savedPessoas.Should().HaveCount(3);
        savedPessoas.Select(p => p.NII).Should().BeEquivalentTo("11111", "22222", "33333");
        savedPessoas.Select(p => p.ExternalId).Should().BeEquivalentTo("NEW001", "OLD002", "NEW002");
    }

    [Fact]
    public async Task ShouldReturnInternalServerError_WhenSoapServiceThrowsException()
    {
        // Arrange
        _mockSoapChannel
            .Setup(c => c.ZhrWsGetPernrAsync(It.IsAny<ZhrWsGetPernrRequest>()))
            .ThrowsAsync(new Exception("SOAP service unavailable"));

        // Act
        var response = await _client.PostAsync("/api/pessoas/import", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        var savedPessoas = await _context.Pessoas.AsNoTracking().ToListAsync();
        savedPessoas.Should().BeEmpty();

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");
        problemDetails.Should().NotBeNull();
        problemDetails.Status.Should().Be(StatusCodes.Status500InternalServerError);
        problemDetails.Title.Should().NotBeNullOrWhiteSpace();
        problemDetails.Type.Should().Be("Exception");
    }

    [Fact]
    public async Task ShouldReturnForbidden_WhenImportAsViewer()
    {
        // Arrange
        using var viewerClient = _factory.CreateAuthenticatedClient(Roles.Viewer);

        // Act
        var response = await viewerClient.PostAsync("/api/pessoas/import", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ShouldReturnUnauthorized_WhenImportUnauthenticated()
    {
        // Arrange
        using var unauthClient = _factory.CreateClient();

        // Act
        var response = await unauthClient.PostAsync("/api/pessoas/import", null);

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