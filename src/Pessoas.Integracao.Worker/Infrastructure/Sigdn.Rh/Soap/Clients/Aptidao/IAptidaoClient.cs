using Pessoas.Integracao.Core.Application.Models;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Generated.Output;

namespace Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Clients.Aptidao;

public interface IAptidaoClient
{
    Task<Dictionary<PessoaImportKey, ZhrSAptidaoOutput?>> GetAptidaoAsync(IReadOnlyList<PessoaImportKey> importKeys, CancellationToken cancellationToken);
}
