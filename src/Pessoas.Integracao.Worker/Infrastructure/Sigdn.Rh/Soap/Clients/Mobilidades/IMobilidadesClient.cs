using Pessoas.Integracao.Core.Application.Models;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Generated.Output;

namespace Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Clients.Mobilidades;

public interface IMobilidadesClient
{
    Task<Dictionary<PessoaImportKey, ZhrSMobilidadesOutput?>> GetMobilidadesAsync(IReadOnlyList<PessoaImportKey> importKey, CancellationToken cancellationToken);
}
