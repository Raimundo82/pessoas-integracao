using FluentAssertions;

using Moq;

using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;
using Pessoas.Integracao.Sync.Domain.Entities;
using Pessoas.Integracao.Sync.Infrastructure.Clients;
using Pessoas.Integracao.Sync.Infrastructure.Services.ZhrSyncronizer.Syncronizers;

namespace Pessoas.Integracao.Sync.Tests.Unit.ZhrSyncronizer;

public sealed class ZhrAptidaoSyncronizerUnitTests
{
    private readonly Mock<IZhrWsGenericClient> _clientMock = new();

    [Fact]
    public async Task ShouldPopulateAptidaoOutputs_WhenResponseContainsData()
    {
        // Arrange
        var aptidao = ZhrAptidaoTestData.ValidOutput();
        SetupResponse([aptidao]);
        var sut = CreateSut();

        // Act
        var result = await sut.FetchAsync(SomePessoaSyncRefs(), ct: TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeAssignableTo<ZhrSBaseModelOutput[]>();
        await Verify(result);
    }

    [Fact]
    public async Task ShouldPopulateEmptyAptidaoOutputs_WhenResponseIsNull()
    {
        // Arrange
        SetupResponse(null);

        var sut = CreateSut();

        // Act
        var result = await sut.FetchAsync(SomePessoaSyncRefs(), ct: TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeAssignableTo<ZhrSBaseModelOutput[]>();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ShouldPopulateEmptyAptidaoOutputs_WhenOutputIsNull()
    {
        // Arrange
        SetupResponse(null);
        var sut = CreateSut();

        // Act
        var result = await sut.FetchAsync(SomePessoaSyncRefs(), ct: TestContext.Current.CancellationToken);

        // Assert
        // Assert
        result.Should().BeOfType<ZhrSBaseModelOutput[]>();
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
                It.IsAny<Func<zhr_wsClient, ZhrWsInputStruct[], Task<ZhrWsAptidaoResponse1?>>>(),
                It.IsAny<Func<ZhrWsAptidaoResponse1?, ZhrSBaseModelOutput[]?>>(),
                refs,
                null,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ShouldCallClientWithReferenceDate_WhenReferenceDateIsProvided()
    {
        // Arrange
        var refs = SomePessoaSyncRefs();
        var referenceDate = new DateOnly(2025, 01, 15);
        SetupResponse(null);
        var sut = CreateSut();

        // Act
        await sut.FetchAsync(
            refs,
            referenceDate,
            ct: TestContext.Current.CancellationToken);

        // Assert
        _clientMock.Verify(c => c.CallAsync(
                It.IsAny<Func<zhr_wsClient, ZhrWsInputStruct[], Task<ZhrWsAptidaoResponse1?>>>(),
                It.IsAny<Func<ZhrWsAptidaoResponse1?, ZhrSBaseModelOutput[]?>>(),
                refs,
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

        Func<zhr_wsClient, ZhrWsInputStruct[], Task<ZhrWsAptidaoResponse1?>> capturedOperation = null!;

        _clientMock
            .Setup(c => c.CallAsync(
                It.IsAny<Func<zhr_wsClient, ZhrWsInputStruct[], Task<ZhrWsAptidaoResponse1?>>>(),
                It.IsAny<Func<ZhrWsAptidaoResponse1?, ZhrSBaseModelOutput[]?>>(),
                It.IsAny<IReadOnlyList<PessoaSyncRef>>(),
                It.IsAny<DateOnly?>(),
                It.IsAny<CancellationToken>()))
            .Callback<Func<zhr_wsClient, ZhrWsInputStruct[], Task<ZhrWsAptidaoResponse1?>>,
                      Func<ZhrWsAptidaoResponse1?, ZhrSBaseModelOutput[]?>,
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

        Func<ZhrWsAptidaoResponse1?, ZhrSBaseModelOutput[]?> capturedSelector = null!;

        _clientMock
            .Setup(c => c.CallAsync(
                It.IsAny<Func<zhr_wsClient, ZhrWsInputStruct[], Task<ZhrWsAptidaoResponse1?>>>(),
                It.IsAny<Func<ZhrWsAptidaoResponse1?, ZhrSBaseModelOutput[]?>>(),
                It.IsAny<IReadOnlyList<PessoaSyncRef>>(),
                It.IsAny<DateOnly?>(),
                It.IsAny<CancellationToken>()))
            .Callback<Func<zhr_wsClient, ZhrWsInputStruct[], Task<ZhrWsAptidaoResponse1?>>,
                      Func<ZhrWsAptidaoResponse1?, ZhrSBaseModelOutput[]?>,
                      IReadOnlyList<PessoaSyncRef>,
                      DateOnly?,
                      CancellationToken>
                      ((op, sel, r, d, ct) => capturedSelector = sel)
            .ReturnsAsync([]);

        // Act
        await sut.FetchAsync(refs, ct: TestContext.Current.CancellationToken);

        // Assert
        var mockSoapResponse = new ZhrWsAptidaoResponse1
        {
            ZhrWsAptidaoResponse = new ZhrWsAptidaoResponse
            {
                Output = [ZhrAptidaoTestData.ValidOutput()]
            }
        };

        var result = capturedSelector(mockSoapResponse);

        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result![0].Should().BeEquivalentTo(ZhrAptidaoTestData.ValidOutput());
    }

    private ZhrAptidaoProvider CreateSut() => new(_clientMock.Object);

    private static List<PessoaSyncRef> SomePessoaSyncRefs() =>
    [
        new() { Ni = "21412", ExternalId = "30005902" }
    ];

    private void SetupResponse(ZhrSBaseModelOutput[]? response)
    {
        _clientMock
            .Setup(c => c.CallAsync(
                It.IsAny<Func<zhr_wsClient, ZhrWsInputStruct[], Task<ZhrWsAptidaoResponse1?>>>(),
                It.IsAny<Func<ZhrWsAptidaoResponse1?, ZhrSBaseModelOutput[]?>>(),
                It.IsAny<IReadOnlyList<PessoaSyncRef>>(),
                It.IsAny<DateOnly?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
    }

    public static class ZhrAptidaoTestData
    {
        public static ZhrSAptidaoOutput ValidOutput() =>
            new()
            {
                Ni = "21412",
                Numsap = "30005902",
                Aptidao =
                [
                    new ZhrSAptidao
                    {
                        Ni = "21412",
                        Subty = "0001",
                        Denominacao = "Exame Médico Geral",
                        AreaExame = "01",
                        ArexamesDesc = "Medicina Geral",
                        ServicoMedInt = "S",
                        Valor = 15.50m,
                        DataExame = "2025-01-15",
                        Modalidade = "NORMAL",
                        ModalDesc = "Normal",
                        Resultado = "AP",
                        ResultadoDesc = "Apto",
                        Observacoes = "Sem restrições"
                    }
                ]
            };
    }


}
