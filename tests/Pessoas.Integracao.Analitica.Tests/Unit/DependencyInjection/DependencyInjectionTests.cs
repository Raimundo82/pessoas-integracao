using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

using Pessoas.Integracao.Analitica.Application.Contracts;

using Pessoas.Integracao.Analitica.Infrastructure;
using Pessoas.Integracao.Analitica.Infrastructure.Repositories;

namespace Pessoas.Integracao.Analitica.Tests.Unit.DependencyInjection;

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
