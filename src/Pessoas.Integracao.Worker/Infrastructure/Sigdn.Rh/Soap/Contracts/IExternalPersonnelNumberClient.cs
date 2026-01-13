using Pessoas.Integracao.Core.Application.DTOs;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Generated.Output;

namespace Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Contracts;

public interface IExternalPersonnelNumberClient
{
    Task<ZhrSListapessoal[]> GetExternalPersonnelNumbersAsync(CancellationToken cancellationToken);
    Task<ZhrSAtribOrgOutput[]> GetExternalPersonnelNumberByNiiAsync(string nii, CancellationToken cancellationToken);
    Task<ZhrSLogMsg[]> GetExternalPersonnelNumberByImportNiisAsync(IReadOnlyList<ImportNiiDto> importNiis, CancellationToken cancellationToken);

}