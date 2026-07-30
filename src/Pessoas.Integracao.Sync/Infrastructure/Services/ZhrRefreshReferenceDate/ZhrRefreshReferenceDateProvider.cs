namespace Pessoas.Integracao.Sync.Infrastructure.Services.ZhrRefreshReferenceDate;

public class ZhrRefreshReferenceDateProvider : IZhrRefreshReferenceDateProvider, IZhrUpdateReferenceDateInitializer
{
    private DateTimeOffset? _referenceDate;

    public void SetReferenceDate(DateTimeOffset referenceDate)
    {
        if (_referenceDate.HasValue) throw new InvalidOperationException("Refresh Reference Date already defined in this scope.");
        _referenceDate = new DateTimeOffset(referenceDate.UtcDateTime.Date, TimeSpan.Zero);
    }

    DateTimeOffset IZhrRefreshReferenceDateProvider.GetReferenceDate()
    {
        return _referenceDate ?? throw new InvalidOperationException("Refresh Reference Date not yet defined in this scope.");
    }
}
