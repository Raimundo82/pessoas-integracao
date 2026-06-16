using System.ServiceModel.Channels;
using System.Text;

using FluentAssertions;

using Microsoft.Extensions.Options;

using Pessoas.Integracao.Sync.Infrastructure.Configuration;
using Pessoas.Integracao.Sync.Infrastructure.Factories;

namespace Pessoas.Integracao.Sync.Tests.Unit.Binding;

public class BindingFactoryTests
{
    [Fact]
    public void ShouldCreateCustomBindingWithDefaultValues_WhenSettingsAreEmpty()
    {
        // Arrange
        var settings = new ZhrWsSettings { };

        // Act
        var binding = new BindingFactory(Options.Create(settings)).CreateBinding();

        // Assert
        binding.Should().NotBeNull();
        binding.Should().BeOfType<CustomBinding>();

        binding.CloseTimeout.Should().Be(TimeSpan.FromSeconds(60));
        binding.OpenTimeout.Should().Be(TimeSpan.FromSeconds(60));
        binding.ReceiveTimeout.Should().Be(TimeSpan.FromSeconds(60));
        binding.SendTimeout.Should().Be(TimeSpan.FromSeconds(60));

        var encodingElement = binding.As<CustomBinding>().Elements.OfType<TextMessageEncodingBindingElement>().Single();
        encodingElement.MessageVersion.Should().Be(MessageVersion.Soap11);
        encodingElement.WriteEncoding.Should().Be(Encoding.UTF8);

        var transportElement = binding.As<CustomBinding>().Elements.OfType<HttpTransportBindingElement>().Single();
        transportElement.MaxBufferSize.Should().Be(int.MaxValue);
        transportElement.MaxReceivedMessageSize.Should().Be(int.MaxValue);
        transportElement.DecompressionEnabled.Should().BeFalse();
        transportElement.UseDefaultWebProxy.Should().BeFalse();
    }

    [Fact]
    public void ShouldCreateCustomBinding_WhenSettingsArePartiallyDefined()
    {
        // Arrange
        var settings = new ZhrWsSettings
        {
            Binding = new WcfBindingSettings
            {
                SoapVersion = "Soap12",
                Encoding = "utf-8",
                OpenTimeoutSeconds = 30,
            }
        };

        // Act
        var binding = new BindingFactory(Options.Create(settings)).CreateBinding();

        // Assert
        binding.Should().NotBeNull();
        binding.Should().BeOfType<CustomBinding>();
        binding.OpenTimeout.Should().Be(TimeSpan.FromSeconds(30));

        var encodingElement = binding.As<CustomBinding>().Elements.OfType<TextMessageEncodingBindingElement>().Single();
        encodingElement.MessageVersion.Should().Be(MessageVersion.Soap12);
        encodingElement.WriteEncoding.Should().Be(Encoding.UTF8);
    }

    [Fact]
    public void ShouldCreateCustomBinding_WhenSettingsAreDefined()
    {
        // Arrange
        var settings = new ZhrWsSettings
        {
            Binding = new WcfBindingSettings
            {
                SoapVersion = "Soap11",
                Encoding = "utf-8",
                MaxBufferSize = 65536,
                MaxReceivedMessageSize = 65536,
                DecompressionEnabled = true,
                UseDefaultWebProxy = true,
                CloseTimeoutSeconds = 30,
                OpenTimeoutSeconds = 30,
                ReceiveTimeoutSeconds = 30,
                SendTimeoutSeconds = 30
            }
        };

        // Act
        var binding = new BindingFactory(Options.Create(settings)).CreateBinding();

        // Assert
        binding.Should().NotBeNull();
        binding.Should().BeOfType<CustomBinding>();
        binding.CloseTimeout.Should().Be(TimeSpan.FromSeconds(30));
        binding.OpenTimeout.Should().Be(TimeSpan.FromSeconds(30));
        binding.ReceiveTimeout.Should().Be(TimeSpan.FromSeconds(30));
        binding.SendTimeout.Should().Be(TimeSpan.FromSeconds(30));

        var encodingElement = binding.As<CustomBinding>().Elements.OfType<TextMessageEncodingBindingElement>().Single();
        encodingElement.MessageVersion.Should().Be(MessageVersion.Soap11);
        encodingElement.WriteEncoding.Should().Be(Encoding.UTF8);

        var transportElement = binding.As<CustomBinding>().Elements.OfType<HttpTransportBindingElement>().Single();
        transportElement.MaxBufferSize.Should().Be(65536);
        transportElement.MaxReceivedMessageSize.Should().Be(65536);
        transportElement.DecompressionEnabled.Should().BeTrue();
        transportElement.UseDefaultWebProxy.Should().BeTrue();
    }

    [Fact]
    public void ShouldFallbackToDefaultValues_WhenSettingsAreInvalid()
    {
        // Arrange
        var settings = new ZhrWsSettings
        {
            Binding = new WcfBindingSettings
            {
                SoapVersion = "InvalidSoapVersion",
                Encoding = "InvalidEncoding",
                CloseTimeoutSeconds = -23,
                OpenTimeoutSeconds = int.MaxValue,
            }
        };

        // Act
        var binding = new BindingFactory(Options.Create(settings)).CreateBinding();


        // Assert
        binding.Should().NotBeNull();
        binding.Should().BeOfType<CustomBinding>();
        binding.CloseTimeout.Should().Be(TimeSpan.FromSeconds(60));
        binding.OpenTimeout.Should().Be(TimeSpan.FromSeconds(60));

        var encodingElement = binding.As<CustomBinding>().Elements.OfType<TextMessageEncodingBindingElement>().Single();
        encodingElement.MessageVersion.Should().Be(MessageVersion.Soap11);
        encodingElement.WriteEncoding.Should().Be(Encoding.UTF8);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(600, 600)]
    [InlineData(0, 60)]
    [InlineData(601, 60)]
    [InlineData(-1, 60)]
    public void ShouldHandleTimeoutBoundaryLimits(int inputSeconds, int expectedSeconds)
    {
        // Arrange
        var settings = new ZhrWsSettings
        {
            Binding = new WcfBindingSettings
            {
                OpenTimeoutSeconds = inputSeconds,
            }
        };

        // Act
        var binding = new BindingFactory(Options.Create(settings)).CreateBinding();

        // Assert
        binding.OpenTimeout.Should().Be(TimeSpan.FromSeconds(expectedSeconds));
    }

    [Theory]
    [InlineData("SOAP11", "Soap11")]
    [InlineData("soap12", "Soap12")]
    [InlineData("sOaP11", "Soap11")]
    public void ShouldBeCaseInsensitiveForSoapVersion(string soapVersion, string expectedVersionName)
    {
        // Arrange
        var settings = new ZhrWsSettings
        {
            Binding = new WcfBindingSettings
            {
                SoapVersion = soapVersion,
            }
        };
        var expectedVersion = expectedVersionName == "Soap12"
            ? MessageVersion.Soap12
            : MessageVersion.Soap11;

        // Act
        var binding = new BindingFactory(Options.Create(settings)).CreateBinding();

        // Assert
        var encodingElement = binding.As<CustomBinding>().Elements.OfType<TextMessageEncodingBindingElement>().Single();
        encodingElement.MessageVersion.Should().Be(expectedVersion);
    }
}
