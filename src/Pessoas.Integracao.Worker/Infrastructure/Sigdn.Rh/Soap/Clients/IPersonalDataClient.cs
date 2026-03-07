using Pessoas.Integracao.Core.Application.Models;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Generated.Output;

namespace Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Clients;

public interface IPersonalDataClient
{
    Task<Dictionary<PessoaImportKey, ZhrSPessoaisOutput?>> GetPersonalDataAsync(IReadOnlyList<PessoaImportKey> importKeys, CancellationToken cancellationToken);
}