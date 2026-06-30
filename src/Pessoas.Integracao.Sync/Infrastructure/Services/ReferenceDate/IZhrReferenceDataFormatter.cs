namespace Pessoas.Integracao.Sync.Infrastructure.Services.ReferenceDate;

public interface IZhrReferenceDateFormatter
{
    string Format(DateOnly dateReference);
}
