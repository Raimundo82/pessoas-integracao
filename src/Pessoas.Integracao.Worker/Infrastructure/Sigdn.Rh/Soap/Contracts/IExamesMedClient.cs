using Pessoas.Integracao.Core.Application.Models;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Generated.Output;

namespace Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Contracts;

public interface IExamesMedClient
{
    Task<Dictionary<PessoaImportKey, ZhrSExamesMedOutput?>> GetExamesMedAsync(IReadOnlyList<PessoaImportKey> importKey, CancellationToken cancellationToken);
}