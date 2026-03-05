using System.ServiceModel;

using FluentAssertions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Pessoas.Integracao.Core.Application.Contracts;
using Pessoas.Integracao.Worker.Infrastructure.Extensions;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Configuration;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Channel;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Clients;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Contracts;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Generated.Deltas;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Generated.Output;

namespace Pessoas.Integracao.Worker.Tests.Unit.Extensions;

public sealed class DependencyInjectionUnitTests
{
    [Fact]
    public void ShouldRegisterExpectedServices_WhenAddExternalSoapClientServicesIsCalled()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = BuildConfiguration(
            outputUrl: "http://localhost/output",
            deltasUrl: "http://localhost/deltas");

        // Act
        services.AddExternalSoapClientServices(configuration);

        // Assert
        services.Should().ContainSingle(d => d.ServiceType == typeof(ISoapChannelProvider<zhr_wsChannel>) && d.Lifetime == ServiceLifetime.Scoped);
        services.Should().ContainSingle(d => d.ServiceType == typeof(ISoapChannelProvider<ZHR_WS_DELTASChannel>) && d.Lifetime == ServiceLifetime.Scoped);
        services.Should().ContainSingle(d => d.ServiceType == typeof(IPersonnelNumbersClient) && d.ImplementationType == typeof(PersonnelNumberClient));
        services.Should().ContainSingle(d => d.ServiceType == typeof(IPessoasDataProvider) && d.ImplementationType == typeof(SigdnRhPessoasProvider));
        services.Should().ContainSingle(d => d.ServiceType == typeof(IPessoasImportKeyProvider) && d.ImplementationType == typeof(SigdnRhPessoasImportKeysProvider));
    }

    [Fact]
    public void ShouldResolveChannelFactoriesWithExpectedEndpoints_WhenAddExternalSoapClientServicesIsCalled()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = BuildConfiguration(
            outputUrl: "http://localhost/output",
            deltasUrl: "http://localhost/deltas");

        services.AddExternalSoapClientServices(configuration);
        using var serviceProvider = services.BuildServiceProvider();

        // Act
        var outputFactory = serviceProvider.GetRequiredService<ChannelFactory<zhr_wsChannel>>();
        var deltasFactory = serviceProvider.GetRequiredService<ChannelFactory<ZHR_WS_DELTASChannel>>();

        // Assert
        outputFactory.Endpoint.Address.Uri.ToString().Should().Be("http://localhost/output");
        deltasFactory.Endpoint.Address.Uri.ToString().Should().Be("http://localhost/deltas");
    }

    [Fact]
    public void ShouldRegisterChannelFactoriesAsSingleton_WhenAddExternalSoapClientServicesIsCalled()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = BuildConfiguration(
            outputUrl: "http://localhost/output",
            deltasUrl: "http://localhost/deltas");

        services.AddExternalSoapClientServices(configuration);
        using var serviceProvider = services.BuildServiceProvider();

        // Act
        var firstOutputFactory = serviceProvider.GetRequiredService<ChannelFactory<zhr_wsChannel>>();
        var secondOutputFactory = serviceProvider.GetRequiredService<ChannelFactory<zhr_wsChannel>>();

        // Assert
        services.Should().ContainSingle(d => d.ServiceType == typeof(ChannelFactory<zhr_wsChannel>) && d.Lifetime == ServiceLifetime.Singleton);
        services.Should().ContainSingle(d => d.ServiceType == typeof(ChannelFactory<ZHR_WS_DELTASChannel>) && d.Lifetime == ServiceLifetime.Singleton);
        ReferenceEquals(firstOutputFactory, secondOutputFactory).Should().BeTrue();
    }

    [Fact]
    public void ShouldCreateSingletonChannelFactoryWithEndpointSelector_WhenAddSoapChannelFactorySingletonIsCalled()
    {
        // Arrange
        var services = new ServiceCollection();
        services.Configure<DataSourceSettings>(options =>
        {
            options.OutputUrl = "http://localhost/output-isolated";
            options.DeltasUrl = "http://localhost/deltas-isolated";
        });

        services.AddSoapChannelFactorySingleton<zhr_wsChannel>(settings => settings.OutputUrl);
        using var serviceProvider = services.BuildServiceProvider();

        // Act
        var firstFactory = serviceProvider.GetRequiredService<ChannelFactory<zhr_wsChannel>>();
        var secondFactory = serviceProvider.GetRequiredService<ChannelFactory<zhr_wsChannel>>();

        // Assert
        services.Should().ContainSingle(d => d.ServiceType == typeof(ChannelFactory<zhr_wsChannel>) && d.Lifetime == ServiceLifetime.Singleton);
        firstFactory.Endpoint.Address.Uri.ToString().Should().Be("http://localhost/output-isolated");
        ReferenceEquals(firstFactory, secondFactory).Should().BeTrue();
    }

    [Fact]
    public void ShouldNotOverrideExistingChannelFactoryRegistration_WhenAddSoapChannelFactorySingletonIsCalled()
    {
        // Arrange
        var services = new ServiceCollection();
        var preRegisteredFactory = SoapChannelFactory.CreateChannelFactory<zhr_wsChannel>("http://localhost/pre-registered");

        services.AddSingleton(preRegisteredFactory);
        services.Configure<DataSourceSettings>(options => options.OutputUrl = "http://localhost/new-value");

        // Act
        services.AddSoapChannelFactorySingleton<zhr_wsChannel>(settings => settings.OutputUrl);
        using var serviceProvider = services.BuildServiceProvider();
        var resolvedFactory = serviceProvider.GetRequiredService<ChannelFactory<zhr_wsChannel>>();

        // Assert
        services.Count(d => d.ServiceType == typeof(ChannelFactory<zhr_wsChannel>)).Should().Be(1);
        resolvedFactory.Should().BeSameAs(preRegisteredFactory);
        resolvedFactory.Endpoint.Address.Uri.ToString().Should().Be("http://localhost/pre-registered");
    }

    private static IConfiguration BuildConfiguration(string outputUrl, string deltasUrl)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["SigdnRh:DataSource:OutputUrl"] = outputUrl,
                ["SigdnRh:DataSource:DeltasUrl"] = deltasUrl
            })
            .Build();
    }
}