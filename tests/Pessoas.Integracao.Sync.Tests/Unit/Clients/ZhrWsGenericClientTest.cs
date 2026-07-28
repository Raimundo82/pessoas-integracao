using FluentAssertions;

using Microsoft.Extensions.Options;

using Moq;

using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;
using Pessoas.Integracao.Sync.Domain.Entities;
using Pessoas.Integracao.Sync.Infrastructure.Clients;
using Pessoas.Integracao.Sync.Infrastructure.Configuration;
using Pessoas.Integracao.Sync.Infrastructure.Factories;
using Pessoas.Integracao.Sync.Infrastructure.Services.ReferenceDate;

namespace Pessoas.Integracao.Sync.Tests.Unit.Clients;

public class ZhrWsGenericClientTest()
{
    [Fact]
    public async Task ShouldOrchestrateSoapCallCorrectly_WhenValidRequestIsMade()
    {
        // Arrange
        var referenceDate = new DateOnly(2026, 6, 30);
        var formattedDate = "2026-06-30";
        var pessoaSyncRefs = new List<PessoaSyncRef> { new() { Ni = "00001", ExternalId = "3000001" }, };
        var clientFactoryMock = new Mock<IZhrWsGenericClientFactory<zhr_wsClient, zhr_ws>>();

        clientFactoryMock.Setup(f => f.CreateClient()).Returns(new zhr_wsClient());

        var settingsMock = new Mock<IOptions<ZhrWsSettings>>();
        var settings = new ZhrWsSettings { Empresa = "TestEmpresa" };
        settingsMock.Setup(s => s.Value).Returns(settings);

        var referenceDateFormatterMock = new Mock<IZhrReferenceDateFormatter>();
        referenceDateFormatterMock
            .Setup(f => f.Format(referenceDate))
            .Returns(formattedDate);

        var uut = new ZhrWsGenericClient(clientFactoryMock.Object, settingsMock.Object, referenceDateFormatterMock.Object);
        ZhrWsInputStruct[] capturedInputs = [];

        var expectedResponse = new ZhrWsAptidaoResponse1
        {
            ZhrWsAptidaoResponse = new ZhrWsAptidaoResponse
            {
                Output = [new() { Ni = "00001", Numsap = "3000001" }]
            }
        };

        Task<ZhrWsAptidaoResponse1?> DelegatedFunc(zhr_wsClient client, ZhrWsInputStruct[] inputs)
        {
            capturedInputs = inputs;
            return Task.FromResult<ZhrWsAptidaoResponse1?>(expectedResponse);
        }

        // Act
        var result = await uut.CallAsync(
            DelegatedFunc,
            (response) => response.ZhrWsAptidaoResponse,
            pessoaSyncRefs,
            ct: TestContext.Current.CancellationToken,
            referenceDate: referenceDate
        );

        // Assert
        result.Should().BeSameAs(expectedResponse.ZhrWsAptidaoResponse);
        capturedInputs.Should().NotBeNull();
        capturedInputs.Should().HaveCount(1);
        var input = capturedInputs[0];
        input.Ni.Should().Be("00001");
        input.Numsap.Should().Be("3000001");
        input.Empresa.Should().Be("TestEmpresa");
        input.Dtreferencia.Should().Be(formattedDate);
        referenceDateFormatterMock.Verify(f => f.Format(referenceDate), Times.Once);
    }

    [Fact]
    public async Task ShouldOrchestrateSoapCallCorrectly_WhenMultipleRefsAreProvidedAndNoReferenceDateIsSpecified()
    {
        // Arrange
        var pessoaSyncRefs = new List<PessoaSyncRef> { new() { Ni = "00001", ExternalId = "3000001" }, new() { Ni = "00002", ExternalId = "3000002" } };
        var clientFactoryMock = new Mock<IZhrWsGenericClientFactory<zhr_wsClient, zhr_ws>>();

        clientFactoryMock.Setup(f => f.CreateClient()).Returns(new zhr_wsClient());

        var settingsMock = new Mock<IOptions<ZhrWsSettings>>();
        var settings = new ZhrWsSettings { Empresa = "TestEmpresa" };
        settingsMock.Setup(s => s.Value).Returns(settings);

        var referenceDateFormatterMock = new Mock<IZhrReferenceDateFormatter>();

        var uut = new ZhrWsGenericClient(clientFactoryMock.Object, settingsMock.Object, referenceDateFormatterMock.Object);
        ZhrWsInputStruct[] capturedInputs = [];

        var expectedResponse = new ZhrWsAptidaoResponse1
        {
            ZhrWsAptidaoResponse = new ZhrWsAptidaoResponse
            {
                Output = [new() { Ni = "00001", Numsap = "3000001" }, new() { Ni = "00002", Numsap = "3000002" }]
            }
        };

        Task<ZhrWsAptidaoResponse1?> DelegatedFunc(zhr_wsClient client, ZhrWsInputStruct[] inputs)
        {
            capturedInputs = inputs;
            return Task.FromResult<ZhrWsAptidaoResponse1?>(expectedResponse);
        }

        // Act
        var result = await uut.CallAsync(
            DelegatedFunc,
            (response) => response.ZhrWsAptidaoResponse,
            pessoaSyncRefs,
            ct: TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeSameAs(expectedResponse.ZhrWsAptidaoResponse);
        capturedInputs.Should().NotBeNull();
        capturedInputs.Should().HaveCount(2);
        capturedInputs.Select(i => i.Dtreferencia).Should().AllBeEquivalentTo(string.Empty);
        referenceDateFormatterMock.Verify(f => f.Format(It.IsAny<DateOnly>()), Times.Never);
    }

    [Fact]
    public async Task ShouldOrchestrateSoapCallCorrectly_WhenNoRefsAreProvided()
    {
        // Arrange
        var pessoaSyncRefs = new List<PessoaSyncRef> { };
        var clientFactoryMock = new Mock<IZhrWsGenericClientFactory<zhr_wsClient, zhr_ws>>();

        clientFactoryMock.Setup(f => f.CreateClient()).Returns(new zhr_wsClient());

        var settingsMock = new Mock<IOptions<ZhrWsSettings>>();
        var settings = new ZhrWsSettings { Empresa = "TestEmpresa" };
        settingsMock.Setup(s => s.Value).Returns(settings);

        var referenceDateFormatterMock = new Mock<IZhrReferenceDateFormatter>();

        var uut = new ZhrWsGenericClient(clientFactoryMock.Object, settingsMock.Object, referenceDateFormatterMock.Object);
        var soapOperationMock = new Mock<Func<zhr_wsClient, ZhrWsInputStruct[], Task<ZhrWsAptidaoResponse1?>>>();
        var responseSelectorMock = new Mock<Func<ZhrWsAptidaoResponse1, ZhrWsAptidaoResponse?>>();

        // Act
        var result = await uut.CallAsync(
            soapOperationMock.Object,
             responseSelectorMock.Object,
            pessoaSyncRefs,
            ct: TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeNull();
        soapOperationMock.Verify(op => op(It.IsAny<zhr_wsClient>(), It.IsAny<ZhrWsInputStruct[]>()), Times.Never);
    }

    [Fact]
    public async Task ShouldAbortClient_WhenCancellationTokenIsCancelled()
    {
        // Arrange
        var pessoaSyncRefs = new List<PessoaSyncRef> { new() { Ni = "00001", ExternalId = "3000001" } };
        var clientFactoryMock = new Mock<IZhrWsGenericClientFactory<zhr_wsClient, zhr_ws>>();
        clientFactoryMock.Setup(f => f.CreateClient()).Returns(new zhr_wsClient());

        var settingsMock = new Mock<IOptions<ZhrWsSettings>>();
        settingsMock.Setup(s => s.Value).Returns(new ZhrWsSettings { Empresa = "TestEmpresa" });

        var referenceDateFormatterMock = new Mock<IZhrReferenceDateFormatter>();

        var uut = new ZhrWsGenericClient(clientFactoryMock.Object, settingsMock.Object, referenceDateFormatterMock.Object);

        var cts = new CancellationTokenSource();

        static async Task<ZhrWsAptidaoResponse1?> HangingOperation(zhr_wsClient client, ZhrWsInputStruct[] inputs)
        {
            while (client.State == System.ServiceModel.CommunicationState.Opened)
            {
                await Task.Delay(10);
            }
            return null;
        }
        var responseSelectorMock = new Mock<Func<ZhrWsAptidaoResponse1, ZhrWsAptidaoResponse?>>();

        // Act
        var callTask = uut.CallAsync(
            HangingOperation,
            responseSelectorMock.Object,
            pessoaSyncRefs,
            ct: cts.Token
        );

        cts.Cancel();

        // Assert
        await callTask.As<Task>().WaitAsync(TimeSpan.FromSeconds(2), TestContext.Current.CancellationToken);
        var result = await callTask;
        responseSelectorMock.Verify(op => op(It.IsAny<ZhrWsAptidaoResponse1>()), Times.Never);
        result.Should().BeNull();
    }

    [Fact]
    public async Task ShouldPropagateException_WhenSoapOperationFails()
    {
        // Arrange
        var pessoaSyncRefs = new List<PessoaSyncRef> { new() { Ni = "00001", ExternalId = "3000001" } };
        var clientFactoryMock = new Mock<IZhrWsGenericClientFactory<zhr_wsClient, zhr_ws>>();
        clientFactoryMock.Setup(f => f.CreateClient()).Returns(new zhr_wsClient());

        var settingsMock = new Mock<IOptions<ZhrWsSettings>>();
        settingsMock.Setup(s => s.Value).Returns(new ZhrWsSettings { Empresa = "TestEmpresa" });

        var referenceDateFormatterMock = new Mock<IZhrReferenceDateFormatter>();

        var uut = new ZhrWsGenericClient(clientFactoryMock.Object, settingsMock.Object, referenceDateFormatterMock.Object);

        var expectedException = new System.ServiceModel.FaultException("SOAP Fault");

        Task<ZhrWsAptidaoResponse1?> FailingOperation(zhr_wsClient client, ZhrWsInputStruct[] inputs)
        {
            return Task.FromException<ZhrWsAptidaoResponse1?>(expectedException);
        }
        var responseSelectorMock = new Mock<Func<ZhrWsAptidaoResponse1, ZhrWsAptidaoResponse?>>();

        // Act
        var act = () => uut.CallAsync(
            FailingOperation,
            responseSelectorMock.Object,
            pessoaSyncRefs,
            ct: TestContext.Current.CancellationToken
        );

        // Assert
        await act.Should().ThrowAsync<System.ServiceModel.FaultException>().WithMessage("SOAP Fault");
        responseSelectorMock.Verify(op => op(It.IsAny<ZhrWsAptidaoResponse1>()), Times.Never);

    }

    [Fact]
    public async Task ShouldPopulateEmptyAptidaoOutputs_WhenReponseIsNull()
    {
        // Arrange
        var pessoaSyncRefs = new List<PessoaSyncRef> {
            new() { Ni = "00001", ExternalId = "3000001" },
            new() { Ni = "00002", ExternalId = "3000002" }
        };
        var clientFactoryMock = new Mock<IZhrWsGenericClientFactory<zhr_wsClient, zhr_ws>>();

        clientFactoryMock.Setup(f => f.CreateClient()).Returns(new zhr_wsClient());

        var settingsMock = new Mock<IOptions<ZhrWsSettings>>();
        var settings = new ZhrWsSettings { Empresa = "TestEmpresa" };
        settingsMock.Setup(s => s.Value).Returns(settings);

        var referenceDateFormatterMock = new Mock<IZhrReferenceDateFormatter>();

        var uut = new ZhrWsGenericClient(clientFactoryMock.Object, settingsMock.Object, referenceDateFormatterMock.Object);
        ZhrWsInputStruct[] capturedInputs = [];

        var expectedResponse = new ZhrWsAptidaoResponse1 { ZhrWsAptidaoResponse = null, };

        Task<ZhrWsAptidaoResponse1?> DelegatedFunc(zhr_wsClient client, ZhrWsInputStruct[] inputs)
        {
            capturedInputs = inputs;
            return Task.FromResult<ZhrWsAptidaoResponse1?>(expectedResponse);
        }

        // Act
        var result = await uut.CallAsync(
            DelegatedFunc,
            (response) => response?.ZhrWsAptidaoResponse,
            pessoaSyncRefs,
            ct: TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeNull();
    }
}
