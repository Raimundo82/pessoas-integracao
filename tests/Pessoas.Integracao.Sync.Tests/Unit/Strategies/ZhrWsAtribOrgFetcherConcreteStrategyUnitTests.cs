using FluentAssertions;

using Moq;

using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;
using Pessoas.Integracao.Sync.Domain.Entities;
using Pessoas.Integracao.Sync.Infrastructure.Clients;
using Pessoas.Integracao.Sync.Infrastructure.Providers.FetchResults;
using Pessoas.Integracao.Sync.Infrastructure.Strategies;

namespace Pessoas.Integracao.Sync.Tests.Unit.Strategies;

public class ZhrWsAtribOrgFetcherConcreteStrategyUnitTests
{
    private readonly Mock<IZhrWsGenericClient> _clientMock = new();

    private ZhrWsAtribOrgFetcherConcreteStrategy CreateSut() =>
        new(_clientMock.Object);

    private static List<PessoaSyncRef> SomeRefs() =>
    [
        new() { Ni = "21412", ExternalId = "30005902" }
    ];

    private void SetupResponse(ZhrWsAtribOrgResponse1? response)
    {
        _clientMock
            .Setup(c => c.CallAsync(
                It.IsAny<Func<zhr_wsClient, ZhrWsInputStruct[], Task<ZhrWsAtribOrgResponse1?>>>(),
                It.IsAny<IReadOnlyCollection<PessoaSyncRef>>(),
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

    [Fact]
    public async Task ShouldPopulateAtribOrgOutputs_WhenResponseContainsData()
    {
        // Arrange
        var atribOrg = ZhrAtribOrgTestData.ValidOutput();

        SetupResponse(new ZhrWsAtribOrgResponse1
        {
            ZhrWsAtribOrgResponse = new ZhrWsAtribOrgResponse
            {
                Output = [atribOrg]
            }
        });

        var sut = CreateSut();

        // Act
        var result = await sut.FetchAsync(
            SomeRefs(),
            null,
            CancellationToken.None);

        // Assert
        result.Should().BeOfType<AtribOrgFetchResult>();
        var atribOrgResult = (AtribOrgFetchResult)result;
        await Verify(atribOrgResult.Data);
    }

    [Fact]
    public async Task ShouldPopulateEmptyAtribOrgOutputs_WhenResponseIsNull()
    {
        // Arrange
        SetupResponse(null);

        var sut = CreateSut();

        // Act
        var result = await sut.FetchAsync(
            SomeRefs(),
            null,
            CancellationToken.None);

        // Assert
        result.Should().BeOfType<AtribOrgFetchResult>();
        var atribOrgResult = (AtribOrgFetchResult)result;
        atribOrgResult.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task ShouldPopulateEmptyAtribOrgOutputs_WhenInnerResponseIsNull()
    {
        // Arrange
        SetupResponse(new ZhrWsAtribOrgResponse1
        {
            ZhrWsAtribOrgResponse = null
        });

        var sut = CreateSut();

        // Act
        var result = await sut.FetchAsync(
            SomeRefs(),
            null,
            CancellationToken.None);

        // Assert
        result.Should().BeOfType<AtribOrgFetchResult>();
        var atribOrgResult = (AtribOrgFetchResult)result;
        atribOrgResult.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task ShouldPopulateEmptyAtribOrgOutputs_WhenOutputIsNull()
    {
        // Arrange
        SetupResponse(new ZhrWsAtribOrgResponse1
        {
            ZhrWsAtribOrgResponse = new ZhrWsAtribOrgResponse
            {
                Output = null
            }
        });

        var sut = CreateSut();

        // Act
        var result = await sut.FetchAsync(
            SomeRefs(),
            null,
            CancellationToken.None);

        // Assert
        result.Should().BeOfType<AtribOrgFetchResult>();
        var atribOrgResult = (AtribOrgFetchResult)result;
        atribOrgResult.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task ShouldPassSameRefsAndNullReferenceDateToClient()
    {
        // Arrange
        var refs = SomeRefs();

        SetupResponse(null);

        var sut = CreateSut();

        // Act
        await sut.FetchAsync(
            refs,
            null,
            CancellationToken.None);

        // Assert
        _clientMock.Verify(c => c.CallAsync(
                It.IsAny<Func<zhr_wsClient, ZhrWsInputStruct[], Task<ZhrWsAtribOrgResponse1?>>>(),
                refs,
                null,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ShouldPassReferenceDateToClient()
    {
        // Arrange
        var referenceDate = new DateOnly(2025, 01, 15);

        SetupResponse(null);

        var sut = CreateSut();

        // Act
        await sut.FetchAsync(
            SomeRefs(),
            referenceDate,
            CancellationToken.None);

        // Assert
        _clientMock.Verify(c => c.CallAsync(
                It.IsAny<Func<zhr_wsClient, ZhrWsInputStruct[], Task<ZhrWsAtribOrgResponse1?>>>(),
                It.IsAny<IReadOnlyCollection<PessoaSyncRef>>(),
                referenceDate,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
