using System.Globalization;

using FluentValidation;

namespace Pessoas.Integracao.Sync.Infrastructure.Configuration;

public class ZhrWsSettingsValidator : AbstractValidator<ZhrWsSettings>
{
    private readonly IFormatProvider _formatProvider;
    public ZhrWsSettingsValidator() : this(CultureInfo.InvariantCulture)
    {
    }

    public ZhrWsSettingsValidator(IFormatProvider formatProvider)
    {
        _formatProvider = formatProvider;

        RuleFor(x => x.DateFormat)
            .NotEmpty()
            .WithMessage("ZhrWsSettings:DateFormat is required.")
            .Must(BeAValidDateFormat)
            .WithMessage(x => $"The date format '{x.DateFormat}' is invalid.");

        RuleFor(x => x.Empresa)
            .NotEmpty()
            .WithMessage("ZhrWsSettings:Empresa is required.");

        RuleFor(x => x.Endpoints.BaseUrl)
            .NotEmpty()
            .WithMessage("ZhrWsSettings:Endpoints:BaseUrl is required.");
    }

    private bool BeAValidDateFormat(string format)
    {
        try
        {
            DateTime.Now.ToString(format, _formatProvider);
            return true;
        }
        catch
        {
            return false;
        }
    }
}


