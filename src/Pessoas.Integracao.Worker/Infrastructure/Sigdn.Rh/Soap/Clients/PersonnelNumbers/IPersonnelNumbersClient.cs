using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Generated.Output;

namespace Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Clients;

public interface IPersonnelNumbersClient
{
    Task<ZhrSListapessoal[]> GetPersonnelNumbersAsync(CancellationToken cancellationToken);
}
