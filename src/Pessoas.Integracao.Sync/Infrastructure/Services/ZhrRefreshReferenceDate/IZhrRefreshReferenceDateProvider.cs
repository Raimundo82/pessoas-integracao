namespace Pessoas.Integracao.Sync.Infrastructure.Services.ZhrRefreshReferenceDate;

public interface IZhrRefreshReferenceDateProvider
{
    DateTimeOffset GetReferenceDate();
}
