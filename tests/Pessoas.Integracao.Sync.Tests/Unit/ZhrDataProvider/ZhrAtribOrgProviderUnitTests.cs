using FluentAssertions;

using Moq;

using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;
using Pessoas.Integracao.Sync.Domain.Entities;
using Pessoas.Integracao.Sync.Infrastructure.Clients;
using Pessoas.Integracao.Sync.Infrastructure.Services.ZhrDataProvider.Providers;

namespace Pessoas.Integracao.Sync.Tests.Unit.ZhrDataProvider;

public class ZhrAtribOrgProviderUnitTests
{
    private readonly Mock<IZhrWsGenericClient> _clientMock = new();

    [Fact]
    public async Task ShouldPopulateAtribOrgOutputs_WhenResponseContainsData()
    {
        // Arrange
        var atribOrg = ZhrAtribOrgTestData.ValidOutput();
        SetupResponse([atribOrg]);
        var sut = CreateSut();

        // Act
        var result = await sut.FetchAsync(SomePessoaSyncRefs(), ct: TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeOfType<ZhrSAtribOrgOutput[]>();
        await Verify(result);
    }

    [Fact]
    public async Task ShouldPopulateEmptyAtribOrgOutputs_WhenResponseIsNull()
    {
        // Arrange
        SetupResponse(null);

        var sut = CreateSut();

        // Act
        var result = await sut.FetchAsync(
            SomePessoaSyncRefs(),
            null,
            CancellationToken.None);

        // Assert
        result.Should().BeAssignableTo<ZhrSBaseModelOutput[]>();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ShouldPopulateEmptyAtribOrgOutputs_WhenOutputIsNull()
    {
        // Arrange
        SetupResponse(null);
        var sut = CreateSut();

        // Act
        var result = await sut.FetchAsync(
            SomePessoaSyncRefs(),
            null,
            CancellationToken.None);

        // Assert
        result.Should().BeAssignableTo<ZhrSBaseModelOutput[]>();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ShouldCallClientWithSameRefsAndNullDate_WhenFetchAsyncIsCalled()
    {
        // Arrange
        var refs = SomePessoaSyncRefs();
        SetupResponse(null);
        var sut = CreateSut();

        // Act
        await sut.FetchAsync(refs, ct: TestContext.Current.CancellationToken);

        // Assert
        _clientMock.Verify(c => c.CallAsync(
                It.IsAny<Func<zhr_wsClient, ZhrWsInputStruct[], Task<ZhrWsAtribOrgResponse1?>>>(),
                It.IsAny<Func<ZhrWsAtribOrgResponse1?, ZhrSBaseModelOutput[]?>>(),
                refs,
                null,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ShouldCallClientWithReferenceDate_WhenReferenceDateIsProvided()
    {
        // Arrange
        var referenceDate = new DateOnly(2025, 01, 15);
        SetupResponse(null);
        var sut = CreateSut();

        // Act
        await sut.FetchAsync(
            SomePessoaSyncRefs(),
            referenceDate,
            ct: TestContext.Current.CancellationToken);

        // Assert
        _clientMock.Verify(c => c.CallAsync(
                It.IsAny<Func<zhr_wsClient, ZhrWsInputStruct[], Task<ZhrWsAtribOrgResponse1?>>>(),
                It.IsAny<Func<ZhrWsAtribOrgResponse1?, ZhrSBaseModelOutput[]?>>(),
                It.IsAny<IReadOnlyList<PessoaSyncRef>>(),
                referenceDate,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ShouldUseCorrectOperation_WhenFetchAsyncIsCalled()
    {
        // Arrange
        var refs = SomePessoaSyncRefs();
        var sut = CreateSut();

        Func<zhr_wsClient, ZhrWsInputStruct[], Task<ZhrWsAtribOrgResponse1?>> capturedOperation = null!;

        _clientMock
            .Setup(c => c.CallAsync(
                It.IsAny<Func<zhr_wsClient, ZhrWsInputStruct[], Task<ZhrWsAtribOrgResponse1?>>>(),
                It.IsAny<Func<ZhrWsAtribOrgResponse1?, ZhrSBaseModelOutput[]?>>(),
                It.IsAny<IReadOnlyList<PessoaSyncRef>>(),
                It.IsAny<DateOnly?>(),
                It.IsAny<CancellationToken>()))
            .Callback<Func<zhr_wsClient, ZhrWsInputStruct[], Task<ZhrWsAtribOrgResponse1?>>,
                      Func<ZhrWsAtribOrgResponse1?, ZhrSBaseModelOutput[]?>,
                      IReadOnlyList<PessoaSyncRef>,
                      DateOnly?,
                      CancellationToken>
                      ((op, sel, r, d, ct) => capturedOperation = op)
            .ReturnsAsync([]);

        // Act
        await sut.FetchAsync(refs, ct: TestContext.Current.CancellationToken);

        // Assert
        capturedOperation.Should().NotBeNull();
    }

    [Fact]
    public async Task ShouldUseCorrectSelector_WhenFetchAsyncIsCalled()
    {
        // Arrange
        var refs = SomePessoaSyncRefs();
        var sut = CreateSut();

        Func<ZhrWsAtribOrgResponse1?, ZhrSBaseModelOutput[]?> capturedSelector = null!;

        _clientMock
            .Setup(c => c.CallAsync(
                It.IsAny<Func<zhr_wsClient, ZhrWsInputStruct[], Task<ZhrWsAtribOrgResponse1?>>>(),
                It.IsAny<Func<ZhrWsAtribOrgResponse1?, ZhrSBaseModelOutput[]?>>(),
                It.IsAny<IReadOnlyList<PessoaSyncRef>>(),
                It.IsAny<DateOnly?>(),
                It.IsAny<CancellationToken>()))
            .Callback<Func<zhr_wsClient, ZhrWsInputStruct[], Task<ZhrWsAtribOrgResponse1?>>,
                      Func<ZhrWsAtribOrgResponse1?, ZhrSBaseModelOutput[]?>,
                      IReadOnlyList<PessoaSyncRef>,
                      DateOnly?,
                      CancellationToken>
                      ((op, sel, r, d, ct) => capturedSelector = sel)
            .ReturnsAsync([]);

        // Act
        await sut.FetchAsync(refs, ct: TestContext.Current.CancellationToken);

        // Assert
        var mockSoapResponse = new ZhrWsAtribOrgResponse1
        {
            ZhrWsAtribOrgResponse = new ZhrWsAtribOrgResponse
            {
                Output = [ZhrAtribOrgTestData.ValidOutput()]
            }
        };
        var result = capturedSelector(mockSoapResponse);

        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result![0].Should().BeEquivalentTo(ZhrAtribOrgTestData.ValidOutput());
    }

    private ZhrAtribOrgProvider CreateSut() => new(_clientMock.Object);

    private static List<PessoaSyncRef> SomePessoaSyncRefs() =>
    [
        new() { Ni = "21412", ExternalId = "30005902" }
    ];

    private void SetupResponse(ZhrSAtribOrgOutput[]? response)
    {
        _clientMock
            .Setup(c => c.CallAsync(
                It.IsAny<Func<zhr_wsClient, ZhrWsInputStruct[], Task<ZhrWsAtribOrgResponse1?>>>(),
                It.IsAny<Func<ZhrWsAtribOrgResponse1?, ZhrSBaseModelOutput[]?>>(),
                It.IsAny<IReadOnlyList<PessoaSyncRef>>(),
                It.IsAny<DateOnly?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
    }

    public static class ZhrAtribOrgTestData
    {
        public static ZhrSAtribOrgOutput ValidOutput() =>
            new()
            {
                Numsap = "30005902",
                Ni = "21412",

                DataIncorp = "2010-09-01",
                Datquadro = "2012-01-15",
                Datarma = "2008-10-01",
                Dataingresso = "2008-10-01",

                AtribOrg =
                [
                    new ZhrSAtribOrg
                    {
                        Ni = "21412",

                        Unid = "10000001",
                        Abunid = "CMDPERS",
                        DescUni = "Comando do Pessoal",
                        Situnid = "ATV",
                        DescSit = "Ativa",

                        DtPosicao = "2024-01-01",
                        Posicao = "50000001",
                        Sigla = "OFRH",
                        DescPosi = "Oficial Recursos Humanos",

                        Cargo = "70000001",
                        DescCarg = "Chefe Secção",

                        GrpEmpregad = "A",
                        DescEmp = "Oficial",

                        Codsitcargo = "01",
                        DescSubemp = "Quadro Permanente",

                        Dtsitquadro = "2024-01-01",
                        Datapresenta = "2024-01-15",

                        Stat1 = "A",
                        Stat1desc = "Ativo",

                        Stat2 = "N",
                        Stat2desc = "Normal",

                        Descrelemprego = "Efetivo",
                        Begda = "2024-01-01"
                    }
                ],

                MonitPrazos =
                [
                    new ZhrSMonitPrazos
                    {
                        Ni = "21412",
                        Tipodatafim = "Fim Comissão",
                        Dataprev = "2027-01-01"
                    }
                ],

                DataMedida =
                [
                    new ZhrSDataMedida
                    {
                        Ni = "21412",
                        Dar01 = "01",
                        Dat01 = "2024-01-01",
                        DescMedida = "Promoção"
                    }
                ],

                Om =
                [
                    new ZhrSOm
                    {
                        Ni = "21412",
                        Ace = "ACE1",
                        Acedesc = "Agrupamento Central Exemplo",
                        DetalheOrg = "ORG000001",
                        DetalhePos = "POS000001"
                    }
                ],

                ClassifProf =
                [
                    new ZhrSClassifProf
                    {
                        Ni = "21412",

                        Quadro = "QP",
                        Codposto = "OF001",
                        Abposto = "TEN",
                        Desccatprof = "Tenente",

                        Dtposto = "2020-01-01",

                        Classe = "A01",
                        DescClct = "Classe A",

                        Armaserv = "INF",
                        Descarma = "Infantaria",
                        Datarma = "2012-01-01",

                        Especialidade = "RH",
                        Descesp = "Recursos Humanos",
                        DtEspecialidade = "2018-01-01",

                        Quadespdesc = "Quadro Especial RH",

                        Zzgrad = "S",
                        DtZzgrad = "2020-01-01",

                        Abpostograd = "CAP",
                        DescpostGrad = "Capitão",

                        Numordposto = "100",

                        Agrupclassemar = "AGR1",
                        Classemar = "MAR1",
                        Datclassemar = "2021-01-01",

                        Esp1 = "RH1",
                        Esp1Dsc = "Especialidade RH Principal",

                        Esp2 = "RH2",
                        Esp2Dsc = "Especialidade RH Secundária",

                        Esp3 = "RH3",
                        Esp3Dsc = "Especialidade RH Complementar",

                        Codsubespec = "SUBRH",
                        Abrevespec = "Subespecialidade RH",
                        DtSubesp = "2022-01-01",

                        Dataramoclasse = "2020-01-01",
                        Descramoclasse = "Ramo Classe Exemplo",

                        ZclasCod = "10001",
                        ZramoCod = "20001",
                        ZarmaCod = "INF01",
                        ZespfCod = "RH001",
                        ZqespCod = "QRH01",

                        AreaFunc = "GESTAO",
                        AreaFuncDsc = "Gestão de Recursos Humanos",

                        EspOfSar = "SAR01",
                        EspOfSarDsc = "Especialidade Oficial SAR",

                        EspPra = "PRA01",
                        EspPraDsc = "Especialidade Praça",

                        Cargo = "DIRRH",
                        CargoDsc = "Diretor de Recursos Humanos",

                        Begda = "2024-01-01"
                    }
                ]
            };
    }
}
