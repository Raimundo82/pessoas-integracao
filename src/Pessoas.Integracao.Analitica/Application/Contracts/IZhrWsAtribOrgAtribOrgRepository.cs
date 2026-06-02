using Pessoas.Integracao.Analitica.Models;

namespace Pessoas.Integracao.Analitica.Application.Contracts;

public interface IZhrWsAtribOrgAtribOrgRepository
{
    Task ReplaceMatchingByNiAsync(IReadOnlyList<ZhrWsAtribOrgAtribOrg> entities, CancellationToken ct);

    Task ReplaceAllAsync(IReadOnlyList<ZhrWsAtribOrgAtribOrg> entities, CancellationToken ct);
}

