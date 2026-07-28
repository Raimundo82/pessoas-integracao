using Microsoft.Extensions.Options;

using Pessoas.Integracao.Sync.Infrastructure.Configuration;

namespace Pessoas.Integracao.Sync.Infrastructure.Services.ReferenceDate;

public class ZhrReferenceDateFormatter(IOptions<ZhrWsSettings> settings, TimeProvider timeProvider) : IZhrReferenceDateFormatter
{
    private readonly ZhrWsSettings _zhrWsSettings = settings.Value;
    public string Format(DateOnly dateReference)
    {
        var zhrDateFormat = _zhrWsSettings.DateFormat;
        var currentDate = DateOnly.FromDateTime(timeProvider.GetUtcNow().Date);
        if (dateReference > currentDate)
        {
            dateReference = currentDate;
        }
        return dateReference.ToString(zhrDateFormat);
    }
}
