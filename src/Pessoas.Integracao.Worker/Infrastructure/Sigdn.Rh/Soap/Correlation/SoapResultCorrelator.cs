using Pessoas.Integracao.Core.Application.Models;

namespace Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Correlation;

public class SoapResultCorrelator : ISoapResultCorrelator
{
    public Dictionary<string, TOutput?> CorrelateByKey<TOutput>(
            PessoaImportKey[] keys,
            IEnumerable<TOutput>? output,
            Func<TOutput, string?> niiSelector)
            where TOutput : class
    {
        var outputMap = (output ?? [])
            .Select(x => new { Item = x, Nii = niiSelector(x) })
            .Where(x => !string.IsNullOrWhiteSpace(x.Nii))
            .GroupBy(x => x.Nii!)
            .ToDictionary(group => group.Key, group => group.First().Item);

        return keys
            .Select(k => k.Nii)
            .ToDictionary(
                nii => nii,
                nii => outputMap.TryGetValue(nii, out var item) ? item : null
            );
    }
}