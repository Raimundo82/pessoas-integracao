using Pessoas.Integracao.Core.Application.Models;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Generated.Output;

namespace Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Contracts;

public interface IIndicacoesTempClient
{
    Task<Dictionary<PessoaImportKey, ZhrSTemposervOutput?>> GetIndicacoesTempAsync(IReadOnlyList<PessoaImportKey> importKey, CancellationToken cancellationToken);
}
