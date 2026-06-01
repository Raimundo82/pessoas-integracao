using Pessoas.Integracao.Analitica.Models;

namespace Pessoas.Integracao.Analitica.Application.Contracts
{
    public interface IZhrWsAptidaoAptidaoRepository
    {
        Task UpsertByNiiAsync(IReadOnlyList<ZhrWsAptidaoAptidao> entities, CancellationToken ct);

        Task ReplaceAllAsync(IReadOnlyList<ZhrWsAptidaoAptidao> entities, CancellationToken ct);
    }
}
