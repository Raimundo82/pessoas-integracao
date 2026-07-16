using Pessoas.Integracao.Sync.Application.Contracts;
using Pessoas.Integracao.Sync.Domain.Entities;

namespace Pessoas.Integracao.Sync.Infrastructure.Services.ZhrOutputComposition;

public interface IZhrOutputComposer
{
    Task<IReadOnlyList<ZhrOutput>> ComposeAsync(
        IReadOnlyList<PessoaSyncRef> pessoaSyncRefs,
        CancellationToken ct
    );
}
