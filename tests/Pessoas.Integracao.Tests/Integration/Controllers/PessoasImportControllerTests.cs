using System.Net;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using Moq;

using Pessoas.Integracao.Core.Domain.Constants;
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
        var mockChannelFactory = _scope.ServiceProvider.GetRequiredService<Mock<ISoapChannelProvider<zhr_wsChannel>>>();

        mockChannelFactory.Setup(f => f.CreateChannel(It.IsAny<string>()))
            .Returns(_mockSoapChannel.Object);

        _context.Database.EnsureCreated();
    }

    [Fact]
    public async Task Import_WithEmptyDatabase_PersistsAllFromProvider()
    {
        // Arrange
        var soapGetPernrResponse = new[]
        {
            new ZhrSListapessoal { Ni = "22600", Numsap = "30002697", Empresa = "3000" },
            new ZhrSListapessoal { Ni = "21200", Numsap = "30002798", Empresa = "3000" }
        };
        var soapMessageResponse = new[]
        {
            new ZhrSLogMsg { Ni = "22600", Numsap = "30002697", Msgty = "S" },
            new ZhrSLogMsg { Ni = "21200", Numsap = "30002798", Msgty = "S" },
        };
        _mockSoapChannel
            .Setup(c => c.ZhrWsGetPernrAsync(It.IsAny<ZhrWsGetPernrRequest>()))
            .ReturnsAsync(new ZhrWsGetPernrResponse1
            {
                ZhrWsGetPernrResponse = new ZhrWsGetPernrResponse
                {
                    Output = [new ZhrSGetListapessoal { Pessoal = soapGetPernrResponse }]
                }
            });

        _mockSoapChannel
            .Setup(c => c.ZhrWsAtribOrgAsync(It.IsAny<ZhrWsAtribOrgRequest>()))
            .ReturnsAsync(new ZhrWsAtribOrgResponse1
            {
                ZhrWsAtribOrgResponse = new ZhrWsAtribOrgResponse
                {
                    Message = soapMessageResponse
                }
            });

        // Act
        var response = await _client.PostAsync("/api/pessoas/import", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var savedPessoas = await _context.Pessoas.AsNoTracking().ToListAsync();
        savedPessoas.Should().HaveCount(2);
        savedPessoas.Select(p => p.NII).Should().BeEquivalentTo("22600", "21200");
        savedPessoas.Select(p => p.ExternalId).Should().BeEquivalentTo("30002697", "30002798");
    }

    [Fact]
    public async Task Import_WithDatabaseOnlyRecords_PreservesExistingAndAddsFromProvider()
    {
        // Arrange
        var existingPessoas = new[]
        {
            new Pessoa {NII = "11111", ExternalId = "OLD001"},
            new Pessoa {NII = "22222", ExternalId = "OLD002"}
        };
        await _context.Pessoas.AddRangeAsync(existingPessoas);
        await _context.SaveChangesAsync();

        var soapMessageResponse = new[]
        {
            new ZhrSLogMsg { Ni = "11111", Numsap = "OLD001", Msgty = "S" },
            new ZhrSLogMsg { Ni = "22222", Numsap = "OLD002", Msgty = "S" },
        };

        _mockSoapChannel
            .Setup(c => c.ZhrWsGetPernrAsync(It.IsAny<ZhrWsGetPernrRequest>()))
            .ReturnsAsync(new ZhrWsGetPernrResponse1
            {
                ZhrWsGetPernrResponse = new ZhrWsGetPernrResponse
                {
                    Output = [new ZhrSGetListapessoal { Pessoal = [] }]
                }
            });
        _mockSoapChannel
            .Setup(c => c.ZhrWsAtribOrgAsync(It.IsAny<ZhrWsAtribOrgRequest>()))
            .ReturnsAsync(new ZhrWsAtribOrgResponse1
            {
                ZhrWsAtribOrgResponse = new ZhrWsAtribOrgResponse
                {
                    Message = soapMessageResponse
                }
            });

        // Act
        var response = await _client.PostAsync("/api/pessoas/import", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var savedPessoas = await _context.Pessoas.AsNoTracking().ToListAsync();
        savedPessoas.Should().HaveCount(2);
        savedPessoas.Select(p => p.NII).Should().BeEquivalentTo("11111", "22222");
        savedPessoas.Select(p => p.ExternalId).Should().BeEquivalentTo("OLD001", "OLD002");
    }

    [Fact]
    public async Task Import_WithMixedRecords_UpdatesExistingPreservesUntouchedAndAddsNew()
    {
        // Arrange 
        var existingPessoas = new[]
        {
            new Pessoa {NII = "11111", ExternalId = "OLD001"},
            new Pessoa {NII = "22222", ExternalId = "OLD002"}
        };
        await _context.Pessoas.AddRangeAsync(existingPessoas);
        await _context.SaveChangesAsync();

        var soapGetPernrResponse = new[]
        {
            new ZhrSListapessoal { Ni = "11111", Numsap = "NEW001", Empresa = "3000" },
            new ZhrSListapessoal { Ni = "33333", Numsap = "NEW002", Empresa = "3000" }
        };
        var soapMessageResponse = new[]
        {
            new ZhrSLogMsg { Ni = "11111", Numsap = "NEW001", Msgty = "S" },
            new ZhrSLogMsg { Ni = "22222", Numsap = "OLD002", Msgty = "S" },
            new ZhrSLogMsg { Ni = "33333", Numsap = "NEW002", Msgty = "S" }
        };
        _mockSoapChannel
            .Setup(c => c.ZhrWsGetPernrAsync(It.IsAny<ZhrWsGetPernrRequest>()))
            .ReturnsAsync(new ZhrWsGetPernrResponse1
            {
                ZhrWsGetPernrResponse = new ZhrWsGetPernrResponse
                {
                    Output = [new ZhrSGetListapessoal { Pessoal = soapGetPernrResponse }]
                }
            });
        _mockSoapChannel
            .Setup(c => c.ZhrWsAtribOrgAsync(It.IsAny<ZhrWsAtribOrgRequest>()))
            .ReturnsAsync(new ZhrWsAtribOrgResponse1
            {
                ZhrWsAtribOrgResponse = new ZhrWsAtribOrgResponse
                {
                    Message = soapMessageResponse
                }
            });

        // Act
        var response = await _client.PostAsync("/api/pessoas/import", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var savedPessoas = await _context.Pessoas.AsNoTracking().ToListAsync();
        savedPessoas.Should().HaveCount(3);
        savedPessoas.Select(p => p.NII).Should().BeEquivalentTo("11111", "22222", "33333");
        savedPessoas.Select(p => p.ExternalId).Should().BeEquivalentTo("NEW001", "OLD002", "NEW002");
    }

    [Fact]
    public async Task Import_WithEmptyProviderResponse_DatabaseRemainsUnchanged()
    {
        // TODO: Implementar
        throw new NotImplementedException("Teste por implementar");
    }

    [Fact]
    public async Task Import_WithDatabaseContainingOnlyProviderRecords_UpdatesExistingAndAddsNew()
    {
        // TODO: Implementar
        throw new NotImplementedException("Teste por implementar");
    }

    [Fact]
    public async Task Import_WhenSoapServiceThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        _mockSoapChannel
            .Setup(c => c.ZhrWsGetPernrAsync(It.IsAny<ZhrWsGetPernrRequest>()))
            .ThrowsAsync(new Exception("SOAP service unavailable"));
        _mockSoapChannel
           .Setup(c => c.ZhrWsAtribOrgAsync(It.IsAny<ZhrWsAtribOrgRequest>()))
           .ThrowsAsync(new Exception("SOAP service unavailable"));

        // Act
        var response = await _client.PostAsync("/api/pessoas/import", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        var savedPessoas = await _context.Pessoas.AsNoTracking().ToListAsync();
        savedPessoas.Should().BeEmpty();
    }

    [Fact]
    public async Task Import_AsViewer_ReturnsForbidden()
    {
        // Arrange
        using var viewerClient = _factory.CreateAuthenticatedClient(Roles.Viewer);

        // Act
        var response = await viewerClient.PostAsync("/api/pessoas/import", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Import_Unauthenticated_ReturnsUnauthorized()
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