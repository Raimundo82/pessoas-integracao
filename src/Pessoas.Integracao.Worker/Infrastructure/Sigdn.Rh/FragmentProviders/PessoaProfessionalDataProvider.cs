using Pessoas.Integracao.Core.Application.Models;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Fragments;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Contracts;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Translators;

namespace Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.FragmentProviders;

public class PessoaProfissionalDataProvider(IAtribOrgClient atribOrgClient, IColocacoesTranslator colocacoesTranslator) : IPessoaProfessionalDataProvider
{
    private readonly IAtribOrgClient _atribOrgClient = atribOrgClient;
    private readonly IColocacoesTranslator _colocacoesTranslator = colocacoesTranslator;

    public async Task<Dictionary<PessoaImportKey, PessoaProfessionalDataFragment>> GetPessoaProfessionalDataAsync(IReadOnlyList<PessoaImportKey> importKeys, CancellationToken cancellationToken)
    {
        var atribOrgDataOutputMap = await _atribOrgClient.GetAtribOrgAsync(importKeys, cancellationToken);

        return importKeys.ToDictionary(
            key => key,
            key =>
            {
                var colocacoes = _colocacoesTranslator.Translate(atribOrgDataOutputMap[key]);

                return new PessoaProfessionalDataFragment(colocacoes);
            }
        );
    }
}
