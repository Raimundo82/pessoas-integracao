using FluentAssertions;

using Pessoas.Integracao.Analitica.Infrastructure;
using Pessoas.Integracao.Analitica.Infrastructure.Repositories;
using Pessoas.Integracao.Analitica.Application.Contracts;

namespace Pessoas.Integracao.Tests.TestInfrastructure;

public sealed class AnaliticaDependencyInjectionUnitTests
{
    [Fact]
    public void ShouldRegisterExpectedServices_WhenAddRepositoriesIsCalled()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddRepositories();

        // Assert
        services.Should().ContainSingle(d =>
            d.ServiceType == typeof(IAnaliticaRepository<>) &&
            d.ImplementationType == typeof(AnaliticaRepository<>) &&
            d.Lifetime == ServiceLifetime.Scoped);
    }

    [Fact]
    public void ShouldResolveAnaliticaRepositoryWithConcreteType_WhenAddRepositoriesIsCalled()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddRepositories();

        using var serviceProvider = services.BuildServiceProvider();

        // Act & Assert
        var descriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IAnaliticaRepository<>));
        descriptor.Should().NotBeNull();
        descriptor!.ImplementationType.Should().Be(typeof(AnaliticaRepository<>));
    }
}
