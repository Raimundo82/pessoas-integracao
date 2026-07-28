using FluentValidation.TestHelper;

using Pessoas.Integracao.Sync.Infrastructure.Configuration;

namespace Pessoas.Integracao.Sync.Tests.Unit.Configuration;

public class ZhrWsSettingsValidatorTests
{

    [Fact]
    public void ShouldNotHaveAnyValidationErrors_WhenSettingsAreValid()
    {
        // Arrange
        var validator = new ZhrWsSettingsValidator();
        var settings = new ZhrWsSettings
        {
            DateFormat = "yyyy-MM-dd",
            Empresa = "3000",
            Endpoints = new ZhrEndpointSettings { BaseUrl = "http://api.com" }
        };

        // Act
        var result = validator.TestValidate(settings);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ShouldHaveValidationErrorForDateFormat_WhenDateFormatIsEmpty()
    {
        // Arrange
        var validator = new ZhrWsSettingsValidator();
        var settings = CreateValidSettings();
        settings.DateFormat = string.Empty;

        // Act
        var result = validator.TestValidate(settings);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DateFormat)
              .WithErrorMessage("ZhrWsSettings:DateFormat is required.");
    }

    [Fact]
    public void ShouldHaveValidationErrorForDateFormat_WhenDateFormatIsInvalid()
    {
        // Arrange
        var validator = new ZhrWsSettingsValidator();
        var settings = CreateValidSettings();
        settings.DateFormat = "X";

        // Act
        var result = validator.TestValidate(settings);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DateFormat)
              .WithErrorMessage("The date format 'X' is invalid.");
    }

    [Fact]
    public void ShouldHaveValidationErrorForEmpresa_WhenEmpresaIsEmpty()
    {
        // Arrange
        var validator = new ZhrWsSettingsValidator();
        var settings = CreateValidSettings();
        settings.Empresa = string.Empty;

        // Act
        var result = validator.TestValidate(settings);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Empresa)
              .WithErrorMessage("ZhrWsSettings:Empresa is required.");
    }

    [Fact]
    public void ShouldHaveValidationErrorForEndpointsBaseUrl_WhenEndpointsIsNull()
    {
        // Arrange
        var validator = new ZhrWsSettingsValidator();
        var settings = CreateValidSettings();
        settings.Endpoints = null!;

        // Act
        var result = validator.TestValidate(settings);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Endpoints)
              .WithErrorMessage("ZhrWsSettings:Endpoints is required.");
    }


    [Fact]
    public void ShouldHaveValidationErrorForEndpointsBaseUrl_WhenBaseUrlIsEmpty()
    {
        // Arrange
        var validator = new ZhrWsSettingsValidator();
        var settings = CreateValidSettings();
        settings.Endpoints.BaseUrl = string.Empty;

        // Act
        var result = validator.TestValidate(settings);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Endpoints.BaseUrl)
              .WithErrorMessage("ZhrWsSettings:Endpoints:BaseUrl is required.");
    }

    private static ZhrWsSettings CreateValidSettings() => new()
    {
        DateFormat = "yyyy-MM-dd",
        Empresa = "3000",
        Endpoints = new ZhrEndpointSettings { BaseUrl = "http://api.com" }
    };
}
