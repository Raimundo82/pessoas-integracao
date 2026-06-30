using FluentAssertions;

using Microsoft.Extensions.Options;

using Moq;

using Pessoas.Integracao.Sync.Domain.Entities;
using Pessoas.Integracao.Sync.Infrastructure.Clients;
using Pessoas.Integracao.Sync.Infrastructure.Configuration;
using Pessoas.Integracao.Sync.Infrastructure.Factories;
using Pessoas.Integracao.Sync.Infrastructure.Models.Dados;
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
        var clientFactoryMock = new Mock<IZhrWsGenericClientFactory<ZHR_WSClient, ZHR_WS>>();

        clientFactoryMock.Setup(f => f.CreateClient()).Returns(new ZHR_WSClient());

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

        Task<ZhrWsAptidaoResponse1?> DelegatedFunc(ZHR_WSClient client, ZhrWsInputStruct[] inputs)
        {
            capturedInputs = inputs;
            return Task.FromResult<ZhrWsAptidaoResponse1?>(expectedResponse);
        }

        // Act
        var result = await uut.CallAsync(
            DelegatedFunc,
            pessoaSyncRefs,
            ct: TestContext.Current.CancellationToken,
            referenceDate: referenceDate
        );

        // Assert
        result.Should().BeSameAs(expectedResponse);
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
        var clientFactoryMock = new Mock<IZhrWsGenericClientFactory<ZHR_WSClient, ZHR_WS>>();

        clientFactoryMock.Setup(f => f.CreateClient()).Returns(new ZHR_WSClient());

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

        Task<ZhrWsAptidaoResponse1?> DelegatedFunc(ZHR_WSClient client, ZhrWsInputStruct[] inputs)
        {
            capturedInputs = inputs;
            return Task.FromResult<ZhrWsAptidaoResponse1?>(expectedResponse);
        }

        // Act
        var result = await uut.CallAsync(
            DelegatedFunc,
            pessoaSyncRefs,
            ct: TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeSameAs(expectedResponse);
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
        var clientFactoryMock = new Mock<IZhrWsGenericClientFactory<ZHR_WSClient, ZHR_WS>>();

        clientFactoryMock.Setup(f => f.CreateClient()).Returns(new ZHR_WSClient());

        var settingsMock = new Mock<IOptions<ZhrWsSettings>>();
        var settings = new ZhrWsSettings { Empresa = "TestEmpresa" };
        settingsMock.Setup(s => s.Value).Returns(settings);

        var referenceDateFormatterMock = new Mock<IZhrReferenceDateFormatter>();

        var uut = new ZhrWsGenericClient(clientFactoryMock.Object, settingsMock.Object, referenceDateFormatterMock.Object);
        // Mock the delegate to verify it's never called
        var soapOperationMock = new Mock<Func<ZHR_WSClient, ZhrWsInputStruct[], Task<ZhrWsAptidaoResponse1?>>>();


        // Act
        var result = await uut.CallAsync(
            soapOperationMock.Object,
            pessoaSyncRefs,
            ct: TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeNull();
        soapOperationMock.Verify(op => op(It.IsAny<ZHR_WSClient>(), It.IsAny<ZhrWsInputStruct[]>()), Times.Never);

    }

    [Fact]
    public async Task ShouldAbortClient_WhenCancellationTokenIsCancelled()
    {
        // Arrange
        var pessoaSyncRefs = new List<PessoaSyncRef> { new() { Ni = "00001", ExternalId = "3000001" } };
        var clientFactoryMock = new Mock<IZhrWsGenericClientFactory<ZHR_WSClient, ZHR_WS>>();
        clientFactoryMock.Setup(f => f.CreateClient()).Returns(new ZHR_WSClient());

        var settingsMock = new Mock<IOptions<ZhrWsSettings>>();
        settingsMock.Setup(s => s.Value).Returns(new ZhrWsSettings { Empresa = "TestEmpresa" });

        var referenceDateFormatterMock = new Mock<IZhrReferenceDateFormatter>();

        var uut = new ZhrWsGenericClient(clientFactoryMock.Object, settingsMock.Object, referenceDateFormatterMock.Object);

        var cts = new CancellationTokenSource();

        static async Task<ZhrWsAptidaoResponse1?> HangingOperation(ZHR_WSClient client, ZhrWsInputStruct[] inputs)
        {
            while (client.State == System.ServiceModel.CommunicationState.Opened)
            {
                await Task.Delay(10);
            }
            return null;
        }

        // Act
        var callTask = uut.CallAsync(
            HangingOperation,
            pessoaSyncRefs,
            ct: cts.Token
        );

        cts.Cancel();

        // Assert
        await callTask.As<Task>().WaitAsync(TimeSpan.FromSeconds(2), TestContext.Current.CancellationToken);
    }
}
