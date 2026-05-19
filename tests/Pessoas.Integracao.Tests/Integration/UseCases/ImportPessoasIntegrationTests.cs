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
    private readonly Mock<zhr_wsChannel> _soapChannel;
    private readonly Mock<ISoapChannelProvider<zhr_wsChannel>> _mockSoapChannelProvider;
    private readonly Mock<IPessoasDataProvider> _mockPessoasDataProvider;
    private readonly IOptions<DataSourceSettings> _settings;

    public ImportPessoasIntegrationTests(PostgresTestContainerDb db)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(db.ConnectionString)
            .Options;

        _context = new AppDbContext(options);
        _repository = new PessoaRepository(_context);

        _soapChannel = new Mock<zhr_wsChannel>();
        _mockSoapChannelProvider = new Mock<ISoapChannelProvider<zhr_wsChannel>>();
        _mockSoapChannelProvider
            .Setup(f => f.CreateChannel())
            .Returns(_soapChannel.Object);

        _settings = Options.Create(new DataSourceSettings { Empresa = "3000" });

        _mockPessoasDataProvider = new Mock<IPessoasDataProvider>();

        _context.Database.EnsureCreated();
    }

    [Fact]
    public async Task ShouldPersistAllPessoas_WhenDatabaseIsEmpty()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var importKeysProviderResponse = new[]
        {
            new ZhrSListapessoal { Ni = "22600", Numsap = "30002697", Empresa = "3000" },
            new ZhrSListapessoal { Ni = "21200", Numsap = "30002798", Empresa = "3000" }
        };

        _soapChannel
            .Setup(c => c.ZhrWsGetPernrAsync(It.IsAny<ZhrWsGetPernrRequest>()))
            .ReturnsAsync(new ZhrWsGetPernrResponse1
            {
                ZhrWsGetPernrResponse = new ZhrWsGetPernrResponse
                {
                    Output = [new ZhrSGetListapessoal { Pessoal = importKeysProviderResponse }]
                }
            });

        var perNrsClient = new PersonnelNumberClient(_settings, _mockSoapChannelProvider.Object);
        var importKeysProvider = new SigdnRhPessoasImportKeysProvider(perNrsClient);

        _mockPessoasDataProvider
            .Setup(p => p.GetPessoasByImportKeysAsync(It.IsAny<IReadOnlyList<PessoaImportKey>>(), ct))
            .ReturnsAsync([.. importKeysProviderResponse.Select(k => new Pessoa
            {
                NII = k.Ni,
                ExternalId = k.Numsap
            })]);

        var useCase = new ImportPessoas(_repository, _mockPessoasDataProvider.Object, importKeysProvider);

        // Act
        await useCase.ExecuteAsync(ct);

        // Assert
        var savedPessoas = await _context.Pessoas.AsNoTracking().ToListAsync(ct);
        savedPessoas.Should().HaveCount(2);
        savedPessoas.Select(p => p.NII).Should().BeEquivalentTo("22600", "21200");
        savedPessoas.Select(p => p.ExternalId).Should().BeEquivalentTo("30002697", "30002798");
    }

    [Fact]
    public async Task ShouldPersistDistinctPessoas_WhenDatabaseHasDataAndProviderReturnsNewPessoas()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        await _context.Pessoas.AddRangeAsync(
            new Pessoa { NII = "22600", ExternalId = "30001000" },
            new Pessoa { NII = "21200", ExternalId = "30002000" }
        );
        await _context.SaveChangesAsync(ct);

        var providerKeysResponse = new[]
        {
            new ZhrSListapessoal { Ni = "22601", Numsap = "30001001", Empresa = "3000" },
            new ZhrSListapessoal { Ni = "21201", Numsap = "30002001", Empresa = "3000" }
        };
        _soapChannel
            .Setup(c => c.ZhrWsGetPernrAsync(It.IsAny<ZhrWsGetPernrRequest>()))
            .ReturnsAsync(new ZhrWsGetPernrResponse1
            {
                ZhrWsGetPernrResponse = new ZhrWsGetPernrResponse
                {
                    Output = [new ZhrSGetListapessoal { Pessoal = providerKeysResponse }]
                }
            });

        var perNrsClient = new PersonnelNumberClient(_settings, _mockSoapChannelProvider.Object);
        var importKeysProvider = new SigdnRhPessoasImportKeysProvider(perNrsClient);

        _mockPessoasDataProvider
            .Setup(p => p.GetPessoasByImportKeysAsync(It.IsAny<IReadOnlyList<PessoaImportKey>>(), ct))
            .ReturnsAsync((IReadOnlyList<PessoaImportKey> keys, CancellationToken ct) =>
                [.. keys.Select(k => new Pessoa
                {
                    NII = k.Nii,
                    ExternalId = k.ExternalId
                })]
            );

        var useCase = new ImportPessoas(_repository, _mockPessoasDataProvider.Object, importKeysProvider);

        // Act
        await useCase.ExecuteAsync(ct);

        // Assert
        var savedPessoas = await _context.Pessoas.AsNoTracking().ToListAsync(ct);
        savedPessoas.Should().HaveCount(4);
        savedPessoas.Select(p => p.NII).Should().BeEquivalentTo("22600", "21200", "22601", "21201");
        savedPessoas.Select(p => p.ExternalId).Should().BeEquivalentTo("30001000", "30002000", "30001001", "30002001");
    }


    [Fact]
    public async Task ShouldMaintainDatabasePessoas_WhenKeyProviderReturnsEmptyResponse()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        await _context.Pessoas.AddRangeAsync(
            new Pessoa { NII = "22600", ExternalId = "30001000" },
            new Pessoa { NII = "21200", ExternalId = "30002000" }
        );
        await _context.SaveChangesAsync(ct);

        var providerKeysResponse = Array.Empty<ZhrSListapessoal>();

        _soapChannel
            .Setup(c => c.ZhrWsGetPernrAsync(It.IsAny<ZhrWsGetPernrRequest>()))
            .ReturnsAsync(new ZhrWsGetPernrResponse1
            {
                ZhrWsGetPernrResponse = new ZhrWsGetPernrResponse
                {
                    Output = [new ZhrSGetListapessoal { Pessoal = providerKeysResponse }]
                }
            });

        var perNrsClient = new PersonnelNumberClient(_settings, _mockSoapChannelProvider.Object);
        var importKeysProvider = new SigdnRhPessoasImportKeysProvider(perNrsClient);

        _mockPessoasDataProvider
            .Setup(p => p.GetPessoasByImportKeysAsync(It.IsAny<IReadOnlyList<PessoaImportKey>>(), ct))
            .ReturnsAsync((IReadOnlyList<PessoaImportKey> keys, CancellationToken ct) =>
                [.. keys.Select(k => new Pessoa
                {
                    NII = k.Nii,
                    ExternalId = k.ExternalId
                })]
            );

        var useCase = new ImportPessoas(_repository, _mockPessoasDataProvider.Object, importKeysProvider);

        // Act
        await useCase.ExecuteAsync(ct);

        // Assert
        var savedPessoas = await _context.Pessoas.AsNoTracking().ToListAsync(ct);
        savedPessoas.Should().HaveCount(2);
        savedPessoas.Select(p => p.NII).Should().BeEquivalentTo("22600", "21200");
        savedPessoas.Select(p => p.ExternalId).Should().BeEquivalentTo("30001000", "30002000");
    }

    [Fact]
    public async Task ShouldPersistDistinctPessoas_WhenDatabaseAndProviderContainDuplicatedKeys()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;

        await _context.Pessoas.AddRangeAsync(
            new Pessoa { NII = "22600", ExternalId = "30001000" },
            new Pessoa { NII = "21200", ExternalId = "30002000" }
        );
        await _context.SaveChangesAsync(ct);

        var providerKeysResponse = new[]
        {
            new ZhrSListapessoal { Ni = "22600", Numsap = "30001000", Empresa = "3000" },
            new ZhrSListapessoal { Ni = "21200", Numsap = "30002001", Empresa = "3000" }
        };
        _soapChannel
            .Setup(c => c.ZhrWsGetPernrAsync(It.IsAny<ZhrWsGetPernrRequest>()))
            .ReturnsAsync(new ZhrWsGetPernrResponse1
            {
                ZhrWsGetPernrResponse = new ZhrWsGetPernrResponse
                {
                    Output = [new ZhrSGetListapessoal { Pessoal = providerKeysResponse }]
                }
            });

        var perNrsClient = new PersonnelNumberClient(_settings, _mockSoapChannelProvider.Object);
        var importKeysProvider = new SigdnRhPessoasImportKeysProvider(perNrsClient);

        _mockPessoasDataProvider
            .Setup(p => p.GetPessoasByImportKeysAsync(It.IsAny<IReadOnlyList<PessoaImportKey>>(), ct))
            .ReturnsAsync((IReadOnlyList<PessoaImportKey> keys, CancellationToken ct) =>
                [.. keys.Select(k => new Pessoa
                {
                    NII = k.Nii,
                    ExternalId = k.ExternalId
                })]
            );

        var useCase = new ImportPessoas(_repository, _mockPessoasDataProvider.Object, importKeysProvider);

        // Act
        await useCase.ExecuteAsync(ct);

        // Assert
        var savedPessoas = await _context.Pessoas.AsNoTracking().ToListAsync(ct);
        savedPessoas.Should().HaveCount(2);
        savedPessoas.Select(p => p.NII).Should().BeEquivalentTo("22600", "21200");
        savedPessoas.Select(p => p.ExternalId).Should().BeEquivalentTo("30001000", "30002001");
    }

    [Fact]
    public async Task ShouldPersistDistinctPessoas_WhenDatabaseAndProviderContainSomeDuplicatedKeys()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        await _context.Pessoas.AddRangeAsync(
            new Pessoa { NII = "22600", ExternalId = "30001000" },
            new Pessoa { NII = "21200", ExternalId = "30002000" }
        );
        await _context.SaveChangesAsync(ct);

        var providerKeysResponse = new[]
        {
            new ZhrSListapessoal { Ni = "22600", Numsap = "30001000", Empresa = "3000" },
            new ZhrSListapessoal { Ni = "21201", Numsap = "30002001", Empresa = "3000" }
        };
        _soapChannel
            .Setup(c => c.ZhrWsGetPernrAsync(It.IsAny<ZhrWsGetPernrRequest>()))
            .ReturnsAsync(new ZhrWsGetPernrResponse1
            {
                ZhrWsGetPernrResponse = new ZhrWsGetPernrResponse
                {
                    Output = [new ZhrSGetListapessoal { Pessoal = providerKeysResponse }]
                }
            });

        var perNrsClient = new PersonnelNumberClient(_settings, _mockSoapChannelProvider.Object);
        var importKeysProvider = new SigdnRhPessoasImportKeysProvider(perNrsClient);

        _mockPessoasDataProvider
            .Setup(p => p.GetPessoasByImportKeysAsync(It.IsAny<IReadOnlyList<PessoaImportKey>>(), ct))
            .ReturnsAsync((IReadOnlyList<PessoaImportKey> keys, CancellationToken ct) =>
                [.. keys.Select(k => new Pessoa
                {
                    NII = k.Nii,
                    ExternalId = k.ExternalId
                })]
            );

        var useCase = new ImportPessoas(_repository, _mockPessoasDataProvider.Object, importKeysProvider);

        // Act
        await useCase.ExecuteAsync(ct);

        // Assert
        var savedPessoas = await _context.Pessoas.AsNoTracking().ToListAsync(ct);
        savedPessoas.Should().HaveCount(3);
        savedPessoas.Select(p => p.NII).Should().BeEquivalentTo("22600", "21200", "21201");
        savedPessoas.Select(p => p.ExternalId).Should().BeEquivalentTo("30001000", "30002000", "30002001");
    }

    [Fact]
    public async Task ShouldReplaceUpdateExistingPessoaAndKeepUnrelatedData_WhenMatchingPessoaExists()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        await _context.Pessoas.AddRangeAsync(
            new Pessoa { NII = "22600", ExternalId = "OLD" },
            new Pessoa { NII = "99999" }
        );
        await _context.SaveChangesAsync(ct);

        var soapResponse = new[]
        {
             new ZhrSListapessoal { Ni = "22600", Numsap = "30002697", Empresa = "3000" }
         };
        _soapChannel
            .Setup(c => c.ZhrWsGetPernrAsync(It.IsAny<ZhrWsGetPernrRequest>()))
            .ReturnsAsync(new ZhrWsGetPernrResponse1
            {
                ZhrWsGetPernrResponse = new ZhrWsGetPernrResponse
                {
                    Output = [new ZhrSGetListapessoal { Pessoal = soapResponse }]
                }
            });

        var perNrsClient = new PersonnelNumberClient(_settings, _mockSoapChannelProvider.Object);
        var importKeysProvider = new SigdnRhPessoasImportKeysProvider(perNrsClient);

        _mockPessoasDataProvider
            .Setup(p => p.GetPessoasByImportKeysAsync(It.IsAny<IReadOnlyList<PessoaImportKey>>(), ct))
            .ReturnsAsync((IReadOnlyList<PessoaImportKey> keys, CancellationToken ct) =>
                [.. keys.Select(k => new Pessoa
                {
                    NII = k.Nii,
                    ExternalId = k.ExternalId
                })]
            );

        var useCase = new ImportPessoas(_repository, _mockPessoasDataProvider.Object, importKeysProvider);

        // Act
        await useCase.ExecuteAsync(ct);

        // Assert 
        var savedPessoas = await _context.Pessoas.AsNoTracking().ToListAsync(ct);
        savedPessoas.Should().HaveCount(2);
        savedPessoas.Select(p => p.NII).Should().BeEquivalentTo("22600", "99999");
        savedPessoas.Should().ContainSingle(p => p.NII == "22600").Which.ExternalId.Should().Be("30002697");
    }

    [Fact]
    public async Task ShouldKeepExistingData_WhenPessoasProviderReturnsEmpty()
    {
        // Arrange 
        var ct = TestContext.Current.CancellationToken;
        await _context.Pessoas.AddRangeAsync(
            new Pessoa { NII = "22600" },
            new Pessoa { NII = "21200" }
        );
        await _context.SaveChangesAsync(ct);

        _soapChannel
            .Setup(c => c.ZhrWsGetPernrAsync(It.IsAny<ZhrWsGetPernrRequest>()))
            .ReturnsAsync(new ZhrWsGetPernrResponse1
            {
                ZhrWsGetPernrResponse = new ZhrWsGetPernrResponse
                {
                    Output = [new ZhrSGetListapessoal { Pessoal = [] }]
                }
            });

        var perNrsClient = new PersonnelNumberClient(_settings, _mockSoapChannelProvider.Object);
        var importKeysProvider = new SigdnRhPessoasImportKeysProvider(perNrsClient);

        _mockPessoasDataProvider
            .Setup(p => p.GetPessoasByImportKeysAsync(It.IsAny<IReadOnlyList<PessoaImportKey>>(), ct))
            .ReturnsAsync((IReadOnlyList<PessoaImportKey> keys, CancellationToken ct) =>
                [.. keys.Select(k => new Pessoa
                {
                    NII = k.Nii,
                    ExternalId = k.ExternalId
                })]
            );

        var useCase = new ImportPessoas(_repository, _mockPessoasDataProvider.Object, importKeysProvider);

        // Act
        await useCase.ExecuteAsync(ct);

        // Assert
        var savedPessoas = await _context.Pessoas.AsNoTracking().ToListAsync(ct);
        savedPessoas.Should().HaveCount(2);
        savedPessoas.Select(p => p.NII).Should().BeEquivalentTo("22600", "21200");
    }

    [Fact]
    public async Task ShouldKeepExistingDataAndAddNewData_WhenProviderReturnsNewPessoas()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        await _context.Pessoas.AddAsync(new Pessoa { NII = "12345" }, ct);
        await _context.SaveChangesAsync(ct);
        var soapResponse = new[]
        {
            new ZhrSListapessoal { Ni = "22600", Numsap = "30002697", Empresa = "3000" }
        };

        _soapChannel
            .Setup(c => c.ZhrWsGetPernrAsync(It.IsAny<ZhrWsGetPernrRequest>()))
            .ReturnsAsync(new ZhrWsGetPernrResponse1
            {
                ZhrWsGetPernrResponse = new ZhrWsGetPernrResponse
                {
                    Output = [new ZhrSGetListapessoal { Pessoal = soapResponse }]
                }
            });

        var perNrsClient = new PersonnelNumberClient(_settings, _mockSoapChannelProvider.Object);
        var importKeysProvider = new SigdnRhPessoasImportKeysProvider(perNrsClient);

        _mockPessoasDataProvider
            .Setup(p => p.GetPessoasByImportKeysAsync(It.IsAny<IReadOnlyList<PessoaImportKey>>(), ct))
            .ReturnsAsync((IReadOnlyList<PessoaImportKey> keys, CancellationToken ct) =>
                [.. keys.Select(k => new Pessoa
                {
                    NII = k.Nii,
                    ExternalId = k.ExternalId
                })]
            );

        var useCase = new ImportPessoas(_repository, _mockPessoasDataProvider.Object, importKeysProvider);

        // Act
        await useCase.ExecuteAsync(ct);

        // Assert 
        var savedPessoas = await _context.Pessoas.AsNoTracking().ToListAsync(ct);
        savedPessoas.Should().HaveCount(2);
        savedPessoas.Select(p => p.NII).Should().BeEquivalentTo("22600", "12345");
    }
    [Fact]
    public async Task ShouldHandleMixOfExistingAndNewPessoas_WhenProviderReturnsNewAndExistingPessoas()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        await _context.Pessoas.AddRangeAsync(
            new Pessoa { NII = "11111", ExternalId = "OLD1" },
            new Pessoa { NII = "22222", ExternalId = "OLD2" }
        );
        await _context.SaveChangesAsync(ct);

        var soapResponse = new[]
        {
             new ZhrSListapessoal { Ni = "11111", Numsap = "NEW1", Empresa = "3000" },
             new ZhrSListapessoal { Ni = "33333", Numsap = "NEW3", Empresa = "3000" },
             new ZhrSListapessoal { Ni = "44444", Numsap = "NEW4", Empresa = "3000" }
         };
        _soapChannel
            .Setup(c => c.ZhrWsGetPernrAsync(It.IsAny<ZhrWsGetPernrRequest>()))
            .ReturnsAsync(new ZhrWsGetPernrResponse1
            {
                ZhrWsGetPernrResponse = new ZhrWsGetPernrResponse
                {
                    Output = [new ZhrSGetListapessoal { Pessoal = soapResponse }]
                }
            });

        var perNrsClient = new PersonnelNumberClient(_settings, _mockSoapChannelProvider.Object);
        var importKeysProvider = new SigdnRhPessoasImportKeysProvider(perNrsClient);

        _mockPessoasDataProvider
            .Setup(p => p.GetPessoasByImportKeysAsync(It.IsAny<IReadOnlyList<PessoaImportKey>>(), ct))
            .ReturnsAsync((IReadOnlyList<PessoaImportKey> keys, CancellationToken ct) =>
                [.. keys.Select(k => new Pessoa
                {
                    NII = k.Nii,
                    ExternalId = k.ExternalId
                })]
            );

        var useCase = new ImportPessoas(_repository, _mockPessoasDataProvider.Object, importKeysProvider);

        // Act
        await useCase.ExecuteAsync(ct);

        // Assert 
        var savedPessoas = await _context.Pessoas.AsNoTracking().ToListAsync(ct);
        savedPessoas.Should().HaveCount(4);
        savedPessoas.Select(p => p.NII).Should().BeEquivalentTo("11111", "22222", "33333", "44444");
        savedPessoas.Should().ContainSingle(p => p.NII == "11111").Which.ExternalId.Should().Be("NEW1");
        savedPessoas.Should().ContainSingle(p => p.NII == "22222").Which.ExternalId.Should().Be("OLD2");
        savedPessoas.Should().ContainSingle(p => p.NII == "33333").Which.ExternalId.Should().Be("NEW3");
        savedPessoas.Should().ContainSingle(p => p.NII == "44444").Which.ExternalId.Should().Be("NEW4");
    }

    [Fact]
    public async Task ShouldUpdateMultiplePessoas_WhenProviderReturnsMatchingPessoas()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        await _context.Pessoas.AddRangeAsync(
            new Pessoa { NII = "10001", ExternalId = "OLD1" },
            new Pessoa { NII = "10002", ExternalId = "OLD2" },
            new Pessoa { NII = "10003", ExternalId = "OLD3" }
        );
        await _context.SaveChangesAsync(ct);

        var providerResponse = new[]
        {
             new ZhrSListapessoal { Ni = "10001", Numsap = "UPDATED1", Empresa = "3000" },
             new ZhrSListapessoal { Ni = "10002", Numsap = "UPDATED2", Empresa = "3000" },
             new ZhrSListapessoal { Ni = "10003", Numsap = "UPDATED3", Empresa = "3000" }
         };
        _soapChannel
            .Setup(c => c.ZhrWsGetPernrAsync(It.IsAny<ZhrWsGetPernrRequest>()))
            .ReturnsAsync(new ZhrWsGetPernrResponse1
            {
                ZhrWsGetPernrResponse = new ZhrWsGetPernrResponse
                {
                    Output = [new ZhrSGetListapessoal { Pessoal = providerResponse }]
                }
            });

        _mockPessoasDataProvider
            .Setup(p => p.GetPessoasByImportKeysAsync(It.IsAny<IReadOnlyList<PessoaImportKey>>(), ct))
            .ReturnsAsync((IReadOnlyList<PessoaImportKey> keys, CancellationToken ct) =>
                [.. keys.Select(k => new Pessoa
                {
                    NII = k.Nii,
                    ExternalId = k.ExternalId
                })]
            );

        var perNrsClient = new PersonnelNumberClient(_settings, _mockSoapChannelProvider.Object);
        var importKeysProvider = new SigdnRhPessoasImportKeysProvider(perNrsClient);

        var useCase = new ImportPessoas(_repository, _mockPessoasDataProvider.Object, importKeysProvider);

        // Act
        await useCase.ExecuteAsync(ct);

        // Assert 
        var savedPessoas = await _context.Pessoas.AsNoTracking().ToListAsync(ct);
        savedPessoas.Should().HaveCount(3);
        savedPessoas.Should().ContainSingle(p => p.NII == "10001").Which.ExternalId.Should().Be("UPDATED1");
        savedPessoas.Should().ContainSingle(p => p.NII == "10002").Which.ExternalId.Should().Be("UPDATED2");
        savedPessoas.Should().ContainSingle(p => p.NII == "10003").Which.ExternalId.Should().Be("UPDATED3");
    }

    [Fact]
    public async Task ShouldOverwriteExistingData_WhenUpstreamSourceProvidesPartialData()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
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

        await _context.Pessoas.AddAsync(existingPessoa, ct);
        await _context.SaveChangesAsync(ct);

        var providerKeysResponse = new[]
        {
            new ZhrSListapessoal { Ni = "22600", Numsap = "NEW_ID", Empresa = "3000" }
        };
        _soapChannel
            .Setup(c => c.ZhrWsGetPernrAsync(It.IsAny<ZhrWsGetPernrRequest>()))
            .ReturnsAsync(new ZhrWsGetPernrResponse1
            {
                ZhrWsGetPernrResponse = new ZhrWsGetPernrResponse
                {
                    Output = [new ZhrSGetListapessoal { Pessoal = providerKeysResponse }]
                }
            });

        _mockPessoasDataProvider
            .Setup(p => p.GetPessoasByImportKeysAsync(It.IsAny<IReadOnlyList<PessoaImportKey>>(), ct))
            .ReturnsAsync((IReadOnlyList<PessoaImportKey> keys, CancellationToken ct) =>
                [.. keys.Select(k => new Pessoa
                {
                    NII = k.Nii,
                    ExternalId = k.ExternalId
                })]
            );

        var perNrsClient = new PersonnelNumberClient(_settings, _mockSoapChannelProvider.Object);
        var importKeysProvider = new SigdnRhPessoasImportKeysProvider(perNrsClient);

        var useCase = new ImportPessoas(_repository, _mockPessoasDataProvider.Object, importKeysProvider);

        // Act
        await useCase.ExecuteAsync(ct);

        // Assert
        var savedPessoa = await _context.Pessoas.AsNoTracking().SingleAsync(p => p.NII == "22600", ct);
        savedPessoa.ExternalId.Should().Be("NEW_ID");
        savedPessoa.DadosPessoais.Should().NotBeNull();
        savedPessoa.DadosBiometricos.Should().NotBeNull();
        savedPessoa.DadosPessoais.NomeCompleto.Should().BeNull();
        savedPessoa.DadosPessoais.Sobrenome.Should().BeNull();
        savedPessoa.DadosPessoais.Apelidos.Should().BeNull();
        savedPessoa.DadosPessoais.DataNascimento.Should().BeNull();
        savedPessoa.DadosBiometricos.CorDosOlhos.Should().BeNull();
        savedPessoa.DadosBiometricos.AlturaEmCm.Should().BeNull();
    }


    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
