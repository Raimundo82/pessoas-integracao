using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Generated.Output;

namespace Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Contracts;

public interface IExternalPersonnelNumberClient
{
    Task<ZhrSListapessoal[]> GetExternalPersonnelNumbersAsync(CancellationToken cancellationToken);
}