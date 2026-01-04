using System.Net;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using Moq;

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
    public PessoasImportControllerTests(PostgresTestContainerDb db, IntegrationTestWebAppFactory factory)
    {
        _client = factory.CreateClient();
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
    public async Task Import_WithMockedSoapResponse_PersistsAllPessoasToDatabase()
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
        var savedPessoas = await _context.Pessoas.AsNoTracking().ToListAsync();
        savedPessoas.Should().HaveCount(2);
        savedPessoas.Select(p => p.NII).Should().BeEquivalentTo("22600", "21200");
        savedPessoas.Select(p => p.ExternalId).Should().BeEquivalentTo("30002697", "30002798");
    }

    [Fact]
    public async Task Import_WithEmptySoapResponse_ClearsAllPessoasFromDatabase()
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
        var savedPessoas = await _context.Pessoas.AsNoTracking().ToListAsync();
        savedPessoas.Should().BeEmpty();
    }

    [Fact]
    public async Task Import_ReplacesExistingData_WithNewSoapResponse()
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
        var savedPessoas = await _context.Pessoas.AsNoTracking().ToListAsync();
        savedPessoas.Should().HaveCount(2);
        savedPessoas.Select(p => p.NII).Should().BeEquivalentTo("11111", "33333");
        savedPessoas.Select(p => p.ExternalId).Should().BeEquivalentTo("NEW001", "NEW002");
        savedPessoas.Select(p => p.NII).Should().NotContain(["22222"]);
    }

    [Fact]
    public async Task Import_WhenSoapServiceThrowsException_ReturnsInternalServerError()
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
    }

    public void Dispose()
    {
        _context?.Database.EnsureDeleted();
        _context?.Dispose();
        GC.SuppressFinalize(this);
    }
}