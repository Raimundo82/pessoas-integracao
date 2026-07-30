namespace Pessoas.Integracao.Sync.Infrastructure.Services.ZhrRefreshReferenceDate;

internal interface IZhrUpdateReferenceDateInitializer
{
    void SetReferenceDate(DateTimeOffset referenceDate);
}
