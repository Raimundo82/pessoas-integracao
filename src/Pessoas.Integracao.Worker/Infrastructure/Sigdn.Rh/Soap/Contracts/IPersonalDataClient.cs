using Pessoas.Integracao.Core.Application.Models;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Generated.Output;

namespace Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Contracts;

public interface IPersonalDataClient
{
    Task<ZhrSPessoaisOutput[]> GetPersonalDataAsync(PessoaImportKey[] importKey, CancellationToken cancellationToken);
}