using FluentValidation;

namespace Pessoas.Integracao.Sync.Infrastructure.Configuration;

public class ZhrWsEndpointSettingsValidator : AbstractValidator<ZhrEndpointSettings>
{

    public ZhrWsEndpointSettingsValidator()
    {

        RuleFor(endpoint => endpoint.BaseUrl)
            .NotEmpty()
            .WithMessage("ZhrWsSettings:Endpoints:BaseUrl is required.");

    }
}


