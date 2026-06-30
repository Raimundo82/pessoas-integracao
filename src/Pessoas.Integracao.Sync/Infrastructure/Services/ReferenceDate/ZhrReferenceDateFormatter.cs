namespace Pessoas.Integracao.Sync.Infrastructure.Services.ReferenceDate;

public class ZhrReferenceDateFormatter(TimeProvider timeProvider) : IZhrReferenceDateFormatter
{
    public string Format(DateOnly dateReference)
    {
        var currentDate = DateOnly.FromDateTime(timeProvider.GetUtcNow().Date);
        if (dateReference > currentDate)
        {
            dateReference = currentDate;
        }
        return dateReference.ToString("yyyy-MM-dd");
    }
}
