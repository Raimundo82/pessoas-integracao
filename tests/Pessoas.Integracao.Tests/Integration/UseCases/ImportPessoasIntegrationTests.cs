using FluentAssertions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using Moq;

using Pessoas.Integracao.Core.Application.Contracts;
using Pessoas.Integracao.Core.Application.Models;
using Pessoas.Integracao.Core.Application.UseCases;
using Pessoas.Integracao.Core.Domain.Entities;
using Pessoas.Integracao.Core.Domain.Enums;
using Pessoas.Integracao.Core.Domain.ValueObjects;
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
public sealed class ImportPessoasIntegrationTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly PessoaRepository _repository;
    private readonly EfUnitOfWork _uow;
    private readonly Mock<zhr_wsChannel> _providerResponse;
    private readonly Mock<ISoapChannelProvider<zhr_wsChannel>> _mockChannelFactory;
    private readonly Mock<IPessoasDataProvider> _mockDataProvider;
    private readonly IOptions<DataSourceSettings> _settings;

    public ImportPessoasIntegrationTests(PostgresTestContainerDb db)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(db.ConnectionString)
            .Options;

        _context = new AppDbContext(options);
        _repository = new PessoaRepository(_context);
        _uow = new EfUnitOfWork(_context);

        _providerResponse = new Mock<zhr_wsChannel>();
        _mockChannelFactory = new Mock<ISoapChannelProvider<zhr_wsChannel>>();
        _settings = Options.Create(new DataSourceSettings { Empresa = "3000" });

        _mockChannelFactory
            .Setup(f => f.CreateChannel(_settings.Value.OutputUrl))
            .Returns(_providerResponse.Object);

        _mockDataProvider = new Mock<IPessoasDataProvider>();

        _context.Database.EnsureCreated();
    }

    [Fact]
    public async Task ExecuteAsync_GivenEmptyDatabase_PersistsAllPessoasFromProvider()
    {
        // Arrange
        var providerKeysResponse = new[]
        {
            new ZhrSListapessoal { Ni = "22600", Numsap = "30002697", Empresa = "3000" },
            new ZhrSListapessoal { Ni = "21200", Numsap = "30002798", Empresa = "3000" }
        };
        _providerResponse
            .Setup(c => c.ZhrWsGetPernrAsync(It.IsAny<ZhrWsGetPernrRequest>()))
            .ReturnsAsync(new ZhrWsGetPernrResponse1
            {
                ZhrWsGetPernrResponse = new ZhrWsGetPernrResponse
                {
                    Output = [new ZhrSGetListapessoal { Pessoal = providerKeysResponse }]
                }
            });

        var client = new ExternalPersonnelNumberClient(_settings, _mockChannelFactory.Object);
        var keysProvider = new SigdnRhPessoasProvider(client);

        _mockDataProvider
            .Setup(p => p.GetPessoasByImportKeysAsync(It.IsAny<IReadOnlyList<PessoaImportKey>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([.. providerKeysResponse.Select(k => new Pessoa
            {
                NII = k.Ni,
                ExternalId = k.Numsap
            })]);

        var useCase = new ImportPessoas(_repository, _mockDataProvider.Object, keysProvider, _uow);

        // Act
        await useCase.ExecuteAsync(CancellationToken.None);

        // Assert
        var savedPessoas = await _context.Pessoas.AsNoTracking().ToListAsync();
        savedPessoas.Should().HaveCount(2);
        savedPessoas.Select(p => p.NII).Should().BeEquivalentTo("22600", "21200");
        savedPessoas.Select(p => p.ExternalId).Should().BeEquivalentTo("30002697", "30002798");
    }

    [Fact]
    public async Task ExecuteAsync_GivenPopulatedDatabaseAndProviderResponse_PersistsDistinctPessoas()
    {
        // Arrange
        await _context.Pessoas.AddRangeAsync(
            new Pessoa { NII = "22600", ExternalId = "30001000" },
            new Pessoa { NII = "21200", ExternalId = "30002000" }
        );
        await _context.SaveChangesAsync();

        var providerKeysResponse = new[]
        {
            new ZhrSListapessoal { Ni = "22601", Numsap = "30001001", Empresa = "3000" },
            new ZhrSListapessoal { Ni = "21201", Numsap = "30002001", Empresa = "3000" }
        };
        _providerResponse
            .Setup(c => c.ZhrWsGetPernrAsync(It.IsAny<ZhrWsGetPernrRequest>()))
            .ReturnsAsync(new ZhrWsGetPernrResponse1
            {
                ZhrWsGetPernrResponse = new ZhrWsGetPernrResponse
                {
                    Output = [new ZhrSGetListapessoal { Pessoal = providerKeysResponse }]
                }
            });

        var client = new ExternalPersonnelNumberClient(_settings, _mockChannelFactory.Object);
        var keysProvider = new SigdnRhPessoasProvider(client);

        _mockDataProvider
            .Setup(p => p.GetPessoasByImportKeysAsync(It.IsAny<IReadOnlyList<PessoaImportKey>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([.. providerKeysResponse.Select(k => new Pessoa
            {
                NII = k.Ni,
                ExternalId = k.Numsap
            })]);

        var useCase = new ImportPessoas(_repository, _mockDataProvider.Object, keysProvider, _uow);

        // Act
        await useCase.ExecuteAsync(CancellationToken.None);

        // Assert
        var savedPessoas = await _context.Pessoas.AsNoTracking().ToListAsync();
        savedPessoas.Should().HaveCount(4);
        savedPessoas.Select(p => p.NII).Should().BeEquivalentTo("22600", "21200", "22601", "21201");
        savedPessoas.Select(p => p.ExternalId).Should().BeEquivalentTo("30001000", "30002000", "30001001", "30002001");
    }

    [Fact]
    public async Task ExecuteAsync_GivenEmptyProviderResponse_MaintainsDatabasePessoas()
    {
        // Arrange
        await _context.Pessoas.AddRangeAsync(
            new Pessoa { NII = "22600", ExternalId = "30001000" },
            new Pessoa { NII = "21200", ExternalId = "30002000" }
        );
        await _context.SaveChangesAsync();

        var providerKeysResponse = Array.Empty<ZhrSListapessoal>();

        _providerResponse
            .Setup(c => c.ZhrWsGetPernrAsync(It.IsAny<ZhrWsGetPernrRequest>()))
            .ReturnsAsync(new ZhrWsGetPernrResponse1
            {
                ZhrWsGetPernrResponse = new ZhrWsGetPernrResponse
                {
                    Output = [new ZhrSGetListapessoal { Pessoal = providerKeysResponse }]
                }
            });

        var client = new ExternalPersonnelNumberClient(_settings, _mockChannelFactory.Object);
        var keysProvider = new SigdnRhPessoasProvider(client);

        _mockDataProvider
            .Setup(p => p.GetPessoasByImportKeysAsync(It.IsAny<IReadOnlyList<PessoaImportKey>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([.. providerKeysResponse.Select(k => new Pessoa
            {
                NII = k.Ni,
                ExternalId = k.Numsap
            })]);

        var useCase = new ImportPessoas(_repository, _mockDataProvider.Object, keysProvider, _uow);

        // Act
        await useCase.ExecuteAsync(CancellationToken.None);

        // Assert
        var savedPessoas = await _context.Pessoas.AsNoTracking().ToListAsync();
        savedPessoas.Should().HaveCount(2);
        savedPessoas.Select(p => p.NII).Should().BeEquivalentTo("22600", "21200");
        savedPessoas.Select(p => p.ExternalId).Should().BeEquivalentTo("30001000", "30002000");
    }

    [Fact]
    public async Task ExecuteAsync_GivenDuplicatedKeysInDbAndProviderResponse_PersistsDistinctPessoas()
    {
        // Arrange
        await _context.Pessoas.AddRangeAsync(
            new Pessoa { NII = "22600", ExternalId = "30001000" },
            new Pessoa { NII = "21200", ExternalId = "30002000" }
        );
        await _context.SaveChangesAsync();

        var providerKeysResponse = new[]
        {
            new ZhrSListapessoal { Ni = "22600", Numsap = "30001000", Empresa = "3000" },
            new ZhrSListapessoal { Ni = "21200", Numsap = "30002001", Empresa = "3000" }
        };
        _providerResponse
            .Setup(c => c.ZhrWsGetPernrAsync(It.IsAny<ZhrWsGetPernrRequest>()))
            .ReturnsAsync(new ZhrWsGetPernrResponse1
            {
                ZhrWsGetPernrResponse = new ZhrWsGetPernrResponse
                {
                    Output = [new ZhrSGetListapessoal { Pessoal = providerKeysResponse }]
                }
            });

        var client = new ExternalPersonnelNumberClient(_settings, _mockChannelFactory.Object);
        var keysProvider = new SigdnRhPessoasProvider(client);

        _mockDataProvider
            .Setup(p => p.GetPessoasByImportKeysAsync(It.IsAny<IReadOnlyList<PessoaImportKey>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([.. providerKeysResponse.Select(k => new Pessoa
            {
                NII = k.Ni,
                ExternalId = k.Numsap
            })]);

        var useCase = new ImportPessoas(_repository, _mockDataProvider.Object, keysProvider, _uow);

        // Act
        await useCase.ExecuteAsync(CancellationToken.None);

        // Assert
        var savedPessoas = await _context.Pessoas.AsNoTracking().ToListAsync();
        savedPessoas.Should().HaveCount(2);
        savedPessoas.Select(p => p.NII).Should().BeEquivalentTo("22600", "21200");
        savedPessoas.Select(p => p.ExternalId).Should().BeEquivalentTo("30001000", "30002001");
    }

    [Fact]
    public async Task ExecuteAsync_GivenSomeDuplicatedKeysInDbAndProviderResponse_PersistsDistinctPessoas()
    {
        // Arrange
        await _context.Pessoas.AddRangeAsync(
            new Pessoa { NII = "22600", ExternalId = "30001000" },
            new Pessoa { NII = "21200", ExternalId = "30002000" }
        );
        await _context.SaveChangesAsync();

        var providerKeysResponse = new[]
        {
            new ZhrSListapessoal { Ni = "22600", Numsap = "30001000", Empresa = "3000" },
            new ZhrSListapessoal { Ni = "21201", Numsap = "30002001", Empresa = "3000" }
        };
        _providerResponse
            .Setup(c => c.ZhrWsGetPernrAsync(It.IsAny<ZhrWsGetPernrRequest>()))
            .ReturnsAsync(new ZhrWsGetPernrResponse1
            {
                ZhrWsGetPernrResponse = new ZhrWsGetPernrResponse
                {
                    Output = [new ZhrSGetListapessoal { Pessoal = providerKeysResponse }]
                }
            });

        var client = new ExternalPersonnelNumberClient(_settings, _mockChannelFactory.Object);
        var keysProvider = new SigdnRhPessoasProvider(client);

        _mockDataProvider
            .Setup(p => p.GetPessoasByImportKeysAsync(It.IsAny<IReadOnlyList<PessoaImportKey>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([.. providerKeysResponse.Select(k => new Pessoa
            {
                NII = k.Ni,
                ExternalId = k.Numsap
            })]);

        var useCase = new ImportPessoas(_repository, _mockDataProvider.Object, keysProvider, _uow);

        // Act
        await useCase.ExecuteAsync(CancellationToken.None);

        // Assert
        var savedPessoas = await _context.Pessoas.AsNoTracking().ToListAsync();
        savedPessoas.Should().HaveCount(3);
        savedPessoas.Select(p => p.NII).Should().BeEquivalentTo("22600", "21200", "21201");
        savedPessoas.Select(p => p.ExternalId).Should().BeEquivalentTo("30001000", "30002000", "30002001");
    }

    [Fact]
    public async Task ExecuteAsync_UpdatesExistingPessoa_AndKeepsUnrelatedData()
    {
        // Arrange
        await _context.Pessoas.AddRangeAsync(
            new Pessoa { NII = "22600", ExternalId = "OLD" },
            new Pessoa { NII = "99999" }
        );
        await _context.SaveChangesAsync();

        var soapResponse = new[]
        {
             new ZhrSListapessoal { Ni = "22600", Numsap = "30002697", Empresa = "3000" }
         };
        _providerResponse
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
        _mockDataProvider
            .Setup(p => p.GetPessoasByImportKeysAsync(It.IsAny<IReadOnlyList<PessoaImportKey>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([.. soapResponse.Select(k => new Pessoa
            {
                NII = k.Ni,
                ExternalId = k.Numsap
            })]);
        var useCase = new ImportPessoas(_repository, _mockDataProvider.Object, provider, _uow);

        // Act
        await useCase.ExecuteAsync(CancellationToken.None);

        // Assert 
        var savedPessoas = await _context.Pessoas.AsNoTracking().ToListAsync();
        savedPessoas.Should().HaveCount(2);
        savedPessoas.Select(p => p.NII).Should().BeEquivalentTo("22600", "99999");
        savedPessoas.Should().ContainSingle(p => p.NII == "22600").Which.ExternalId.Should().Be("30002697");
    }

    [Fact]
    public async Task ExecuteAsync_WhenProviderReturnsEmpty_KeepsExistingData()
    {
        // Arrange 
        await _context.Pessoas.AddRangeAsync(
            new Pessoa { NII = "22600" },
            new Pessoa { NII = "21200" }
        );
        await _context.SaveChangesAsync();

        _providerResponse
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
        _mockDataProvider
            .Setup(p => p.GetPessoasByImportKeysAsync(It.IsAny<IReadOnlyList<PessoaImportKey>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Pessoa>());

        var useCase = new ImportPessoas(_repository, _mockDataProvider.Object, provider, _uow);

        // Act
        await useCase.ExecuteAsync(CancellationToken.None);

        // Assert
        var savedPessoas = await _context.Pessoas.AsNoTracking().ToListAsync();
        savedPessoas.Should().HaveCount(2);
        savedPessoas.Select(p => p.NII).Should().BeEquivalentTo("22600", "21200");
    }

    [Fact]
    public async Task ExecuteAsync_KeepsExistingData_AndAddsNewDataFromProvider()
    {
        // Arrange
        await _context.Pessoas.AddAsync(new Pessoa { NII = "12345" });
        await _context.SaveChangesAsync();
        var soapResponse = new[]
    {
            new ZhrSListapessoal { Ni = "22600", Numsap = "30002697", Empresa = "3000" }
        };
        _providerResponse
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
        _mockDataProvider
            .Setup(p => p.GetPessoasByImportKeysAsync(It.IsAny<IReadOnlyList<PessoaImportKey>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([.. soapResponse.Select(k => new Pessoa
            {
                NII = k.Ni,
                ExternalId = k.Numsap
            })]);

        var useCase = new ImportPessoas(_repository, _mockDataProvider.Object, provider, _uow);

        // Act
        await useCase.ExecuteAsync(CancellationToken.None);

        // Assert 
        var savedPessoas = await _context.Pessoas.AsNoTracking().ToListAsync();
        savedPessoas.Should().HaveCount(2);
        savedPessoas.Select(p => p.NII).Should().BeEquivalentTo("22600", "12345");
    }
    [Fact]
    public async Task ExecuteAsync_MixOfUpdatesAndInserts_HandlesAllCorrectly()
    {
        // Arrange
        await _context.Pessoas.AddRangeAsync(
            new Pessoa { NII = "11111", ExternalId = "OLD1" },
            new Pessoa { NII = "22222", ExternalId = "OLD2" }
        );
        await _context.SaveChangesAsync();

        var soapResponse = new[]
        {
             new ZhrSListapessoal { Ni = "11111", Numsap = "NEW1", Empresa = "3000" },
             new ZhrSListapessoal { Ni = "33333", Numsap = "NEW3", Empresa = "3000" },
             new ZhrSListapessoal { Ni = "44444", Numsap = "NEW4", Empresa = "3000" }
         };
        _providerResponse
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
        _mockDataProvider
            .Setup(p => p.GetPessoasByImportKeysAsync(It.IsAny<IReadOnlyList<PessoaImportKey>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([.. soapResponse.Select(k => new Pessoa
            {
                NII = k.Ni,
                ExternalId = k.Numsap
            })]);
        var useCase = new ImportPessoas(_repository, _mockDataProvider.Object, provider, _uow);

        // Act
        await useCase.ExecuteAsync(CancellationToken.None);

        // Assert 
        var savedPessoas = await _context.Pessoas.AsNoTracking().ToListAsync();
        savedPessoas.Should().HaveCount(4);
        savedPessoas.Select(p => p.NII).Should().BeEquivalentTo("11111", "22222", "33333", "44444");
        savedPessoas.Should().ContainSingle(p => p.NII == "11111").Which.ExternalId.Should().Be("NEW1");
        savedPessoas.Should().ContainSingle(p => p.NII == "22222").Which.ExternalId.Should().Be("OLD2");
        savedPessoas.Should().ContainSingle(p => p.NII == "33333").Which.ExternalId.Should().Be("NEW3");
        savedPessoas.Should().ContainSingle(p => p.NII == "44444").Which.ExternalId.Should().Be("NEW4");
    }

    [Fact]
    public async Task ExecuteAsync_UpdatesMultiplePessoas_Simultaneously()
    {
        // Arrange
        await _context.Pessoas.AddRangeAsync(
            new Pessoa { NII = "10001", ExternalId = "OLD1" },
            new Pessoa { NII = "10002", ExternalId = "OLD2" },
            new Pessoa { NII = "10003", ExternalId = "OLD3" }
        );
        await _context.SaveChangesAsync();

        var providerResponse = new[]
        {
             new ZhrSListapessoal { Ni = "10001", Numsap = "UPDATED1", Empresa = "3000" },
             new ZhrSListapessoal { Ni = "10002", Numsap = "UPDATED2", Empresa = "3000" },
             new ZhrSListapessoal { Ni = "10003", Numsap = "UPDATED3", Empresa = "3000" }
         };
        _providerResponse
            .Setup(c => c.ZhrWsGetPernrAsync(It.IsAny<ZhrWsGetPernrRequest>()))
            .ReturnsAsync(new ZhrWsGetPernrResponse1
            {
                ZhrWsGetPernrResponse = new ZhrWsGetPernrResponse
                {
                    Output = [new ZhrSGetListapessoal { Pessoal = providerResponse }]
                }
            });

        _mockDataProvider
            .Setup(p => p.GetPessoasByImportKeysAsync(It.IsAny<IReadOnlyList<PessoaImportKey>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([.. providerResponse.Select(k => new Pessoa
            {
                NII = k.Ni,
                ExternalId = k.Numsap
            })]);

        var client = new ExternalPersonnelNumberClient(_settings, _mockChannelFactory.Object);
        var provider = new SigdnRhPessoasProvider(client);
        var useCase = new ImportPessoas(_repository, _mockDataProvider.Object, provider, _uow);

        // Act
        await useCase.ExecuteAsync(CancellationToken.None);

        // Assert 
        var savedPessoas = await _context.Pessoas.AsNoTracking().ToListAsync();
        savedPessoas.Should().HaveCount(3);
        savedPessoas.Should().ContainSingle(p => p.NII == "10001").Which.ExternalId.Should().Be("UPDATED1");
        savedPessoas.Should().ContainSingle(p => p.NII == "10002").Which.ExternalId.Should().Be("UPDATED2");
        savedPessoas.Should().ContainSingle(p => p.NII == "10003").Which.ExternalId.Should().Be("UPDATED3");
    }

    [Fact]
    public async Task ExecuteAsync_UpdatesOnlyExternalId_PreservesDadosPessoaisAndBiometricos()
    {
        // Arrange
        Pessoa existingPessoa = new()
        {
            NII = "22600",
            ExternalId = "30002697",
            DadosPessoais = new DadosPessoais
            {
                Apelidos = "Apelidos",
                NomeCompleto = "Nome Completo",
                Sobrenome = "Sobrenome",
                DataNascimento = new DateOnly(1982, 10, 18)
            },
            DadosBiometricos = new DadosBiometricos
            {
                AlturaEmCm = 176,
                CorDosOlhos = "Castanhos",
                TipoDeSangue = new TipoDeSangue
                {
                    GrupoSanguineo = GrupoSanguineo.O,
                    Rhesus = Rhesus.POSITIVO
                }
            }
        };

        await _context.Pessoas.AddAsync(existingPessoa);
        await _context.SaveChangesAsync();

        var providerKeysResponse = new[]
        {
            new ZhrSListapessoal { Ni = "22600", Numsap = "NEW_ID", Empresa = "3000" }
        };
        _providerResponse
            .Setup(c => c.ZhrWsGetPernrAsync(It.IsAny<ZhrWsGetPernrRequest>()))
            .ReturnsAsync(new ZhrWsGetPernrResponse1
            {
                ZhrWsGetPernrResponse = new ZhrWsGetPernrResponse
                {
                    Output = [new ZhrSGetListapessoal { Pessoal = providerKeysResponse }]
                }
            });

        _mockDataProvider
            .Setup(p => p.GetPessoasByImportKeysAsync(It.IsAny<IReadOnlyList<PessoaImportKey>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([.. providerKeysResponse.Select(k => new Pessoa
            {
                NII = k.Ni,
                ExternalId = k.Numsap
            })]);

        var client = new ExternalPersonnelNumberClient(_settings, _mockChannelFactory.Object);
        var provider = new SigdnRhPessoasProvider(client);
        var useCase = new ImportPessoas(_repository, _mockDataProvider.Object, provider, _uow);

        // Act
        await useCase.ExecuteAsync(CancellationToken.None);

        // Assert
        var savedPessoa = await _context.Pessoas.AsNoTracking().SingleAsync(p => p.NII == "22600");
        savedPessoa.ExternalId.Should().Be("NEW_ID");
        savedPessoa.DadosPessoais.NomeCompleto.Should().Be(existingPessoa.DadosPessoais.NomeCompleto);
        savedPessoa.DadosPessoais.Sobrenome.Should().Be(existingPessoa.DadosPessoais.Sobrenome);
        savedPessoa.DadosPessoais.Apelidos.Should().Be(existingPessoa.DadosPessoais.Apelidos);
        savedPessoa.DadosPessoais.DataNascimento.Should().Be(existingPessoa.DadosPessoais.DataNascimento);
        savedPessoa.DadosBiometricos.CorDosOlhos.Should().Be(existingPessoa.DadosBiometricos.CorDosOlhos);
        savedPessoa.DadosBiometricos.AlturaEmCm.Should().Be(existingPessoa.DadosBiometricos.AlturaEmCm);
    }


    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}