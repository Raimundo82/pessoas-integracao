using FluentAssertions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using Moq;

using Pessoas.Integracao.Core.Application.UseCases;
using Pessoas.Integracao.Core.Domain.Entities;
using Pessoas.Integracao.Core.Infrastructure.Data;
using Pessoas.Integracao.Core.Infrastructure.Persistence;
using Pessoas.Integracao.Core.Infrastructure.Repositories;
using Pessoas.Integracao.Tests.TestInfrastructure;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Configuration;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Channel;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Clients;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Generated.Output;

namespace Pessoas.Integracao.Tests.Integration.UseCases;

[Collection(nameof(PostgresTestDatabaseCollection))]
public sealed class ImportAllPessoasIntegrationTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly PessoaRepository _repository;
    private readonly EfUnitOfWork _uow;
    private readonly Mock<zhr_wsChannel> _mockSoapChannel;
    private readonly Mock<ISoapChannelProvider<zhr_wsChannel>> _mockChannelFactory;
    private readonly IOptions<DataSourceSettings> _settings;

    public ImportAllPessoasIntegrationTests(PostgresTestContainerDb db)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(db.ConnectionString)
            .Options;

        _context = new AppDbContext(options);
        _repository = new PessoaRepository(_context);
        _uow = new EfUnitOfWork(_context);

        _mockSoapChannel = new Mock<zhr_wsChannel>();
        _mockChannelFactory = new Mock<ISoapChannelProvider<zhr_wsChannel>>();
        _settings = Options.Create(new DataSourceSettings { Empresa = "3000" });

        _mockChannelFactory
            .Setup(f => f.CreateChannel(_settings.Value.OutputUrl))
            .Returns(_mockSoapChannel.Object);

        _context.Database.EnsureCreated();
    }

    [Fact]
    public async Task ExecuteAsync_WithRealDatabase_PersistsAllPessoasFromProvider()
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

        var client = new ExternalPersonnelNumberClient(_settings, _mockChannelFactory.Object);
        var provider = new SigdnRhPessoasProvider(client);
        var useCase = new ImportAllPessoas(_repository, provider, _uow);

        // Act
        await useCase.ExecuteAsync(CancellationToken.None);

        // Assert
        var savedPessoas = await _context.Pessoas.AsNoTracking().ToListAsync();
        savedPessoas.Should().HaveCount(2);
        savedPessoas.Select(p => p.NII).Should().BeEquivalentTo("22600", "21200");
        savedPessoas.Select(p => p.ExternalId).Should().BeEquivalentTo("30002697", "30002798");
    }

    [Fact]
    public async Task ExecuteAsync_ReplacesExistingData_WithNewDataFromProvider()
    {
        // Arrange
        await _context.Pessoas.AddAsync(new Pessoa { NII = "99999", ExternalId = "OLD" });
        await _context.SaveChangesAsync();

        var soapResponse = new[]
        {
            new ZhrSListapessoal { Ni = "22600", Numsap = "30002697", Empresa = "3000" }
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

        var client = new ExternalPersonnelNumberClient(_settings, _mockChannelFactory.Object);
        var provider = new SigdnRhPessoasProvider(client);
        var useCase = new ImportAllPessoas(_repository, provider, _uow);

        // Act
        await useCase.ExecuteAsync(CancellationToken.None);

        // Assert 
        var savedPessoas = await _context.Pessoas.AsNoTracking().ToListAsync();
        savedPessoas.Should().ContainSingle();
        savedPessoas[0].NII.Should().Be("22600");
        savedPessoas[0].ExternalId.Should().Be("30002697");
    }

    [Fact]
    public async Task ExecuteAsync_WhenSoapReturnsEmpty_ClearsAllExistingData()
    {
        // Arrange 
        await _context.Pessoas.AddRangeAsync(
            new Pessoa { NII = "22600" },
            new Pessoa { NII = "21200" }
        );
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

        var client = new ExternalPersonnelNumberClient(_settings, _mockChannelFactory.Object);
        var provider = new SigdnRhPessoasProvider(client);
        var useCase = new ImportAllPessoas(_repository, provider, _uow);

        // Act
        await useCase.ExecuteAsync(CancellationToken.None);

        // Assert
        var savedPessoas = await _context.Pessoas.AsNoTracking().ToListAsync();
        savedPessoas.Should().BeEmpty();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}