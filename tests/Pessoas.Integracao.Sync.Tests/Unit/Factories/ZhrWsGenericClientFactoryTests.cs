using FluentAssertions;

using Microsoft.Extensions.Options;

using Moq;

using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;
using Pessoas.Integracao.Sync.Application.ZhrModels.Deltas;
using Pessoas.Integracao.Sync.Application.ZhrModels.Descodificadoras;
using Pessoas.Integracao.Sync.Infrastructure.Configuration;
using Pessoas.Integracao.Sync.Infrastructure.Factories;


namespace Pessoas.Integracao.Sync.Tests.Unit.Factories;

public class ZhrWsGenericClientFactoryTests
{
    [Fact]
    public void ShouldCreateZhrWsClientFactory_WhenUsingZhrWsClientSpecificFactory()
    {
        // Arrange
        var settings = new ZhrWsSettings
        {
            Empresa = "empresa",
            Endpoints = new ZhrEndpointSettings
            {
                BaseUrl = "http://example.com",
                DadosPath = "dadosPath",
                DeltasPath = "deltasPath",
                DescodifPath = "descodifPath"
            },
            Auth = new ZhrAuthenticationSettings
            {
                Username = "user",
                Password = "pass"
            }
        };

        var mockBindingFactory = new Mock<IBindingFactory>();
        mockBindingFactory.Setup(x => x.CreateBinding()).Returns(new System.ServiceModel.BasicHttpBinding());

        var zhrWsClientFactory = new ZhrWsGenericClientFactory<zhr_wsClient, zhr_ws>(
            mockBindingFactory.Object,
            Options.Create(settings),
            s => s.Endpoints.DadosPath,
            (binding, endpoint) => new zhr_wsClient(binding, endpoint));

        // Act
        var client = zhrWsClientFactory.CreateClient();

        // Assert
        client.Should().NotBeNull();
        client.Should().BeOfType<zhr_wsClient>();
        client.Endpoint.Address.Uri.ToString().Should().Be("http://example.com/dadosPath");
        client.ClientCredentials.UserName.UserName.Should().Be("user");
        client.ClientCredentials.UserName.Password.Should().Be("pass");
    }

    [Fact]
    public void ShouldCreateZhrWsDeltasClientFactory_WhenUsingZhrWsDeltasClientSpecificFactory()
    {
        // Arrange
        var settings = new ZhrWsSettings
        {
            Empresa = "empresa",
            Endpoints = new ZhrEndpointSettings
            {
                BaseUrl = "http://example.com",
                DadosPath = "dadosPath",
                DeltasPath = "deltasPath",
                DescodifPath = "descodifPath"
            }
        };

        var mockBindingFactory = new Mock<IBindingFactory>();
        mockBindingFactory.Setup(x => x.CreateBinding()).Returns(new System.ServiceModel.BasicHttpBinding());

        var zhrWsClientFactory = new ZhrWsGenericClientFactory<ZHR_WS_DELTASClient, ZHR_WS_DELTAS>(
            mockBindingFactory.Object,
            Options.Create(settings),
            s => s.Endpoints.DeltasPath,
            (binding, endpoint) => new ZHR_WS_DELTASClient(binding, endpoint));

        // Act
        var client = zhrWsClientFactory.CreateClient();

        // Assert
        client.Should().NotBeNull();
        client.Should().BeOfType<ZHR_WS_DELTASClient>();
        client.Endpoint.Address.Uri.ToString().Should().Be("http://example.com/deltasPath");
        client.ClientCredentials.UserName.UserName.Should().BeEmpty();
        client.ClientCredentials.UserName.Password.Should().BeEmpty();
    }

    [Fact]
    public void ShouldCreateZhrWsDescodifClientFactory_WhenUsingZhrWsDescodifClientSpecificFactory()
    {
        // Arrange
        var settings = new ZhrWsSettings
        {
            Empresa = "empresa",
            Endpoints = new ZhrEndpointSettings
            {
                BaseUrl = "http://example.com",
                DadosPath = "dadosPath",
                DeltasPath = "deltasPath",
                DescodifPath = "descodifPath"
            },
            Auth = new ZhrAuthenticationSettings
            {
                Username = "user",
                Password = ""
            }
        };

        var mockBindingFactory = new Mock<IBindingFactory>();
        mockBindingFactory.Setup(x => x.CreateBinding()).Returns(new System.ServiceModel.BasicHttpBinding());

        var zhrWsClientFactory = new ZhrWsGenericClientFactory<zhr_ws_descodifClient, zhr_ws_descodif>(
            mockBindingFactory.Object,
            Options.Create(settings),
            s => s.Endpoints.DescodifPath,
            (binding, endpoint) => new zhr_ws_descodifClient(binding, endpoint));

        // Act
        var client = zhrWsClientFactory.CreateClient();

        // Assert
        client.Should().NotBeNull();
        client.Should().BeOfType<zhr_ws_descodifClient>();
        client.Endpoint.Address.Uri.ToString().Should().Be("http://example.com/descodifPath");
        client.ClientCredentials.UserName.UserName.Should().Be("user");
        client.ClientCredentials.UserName.Password.Should().BeEmpty();
    }

    [Theory]
    [InlineData("http://domain.com", "path", "http://domain.com/path")]
    [InlineData("http://domain.com/", "/path", "http://domain.com/path")]
    [InlineData("http://domain.com/", "path", "http://domain.com/path")]
    [InlineData("http://domain.com", "/path", "http://domain.com/path")]
    public void ShouldBuildValidUrl(string baseUrl, string path, string expectedUrl)
    {
        // Act
        var result = ZhrWsGenericClientFactory<zhr_wsClient, zhr_ws>.GetUrl(baseUrl, path);

        // Assert
        result.Should().Be(expectedUrl);
    }
}
