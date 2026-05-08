using Pessoas.Integracao.Core.Application.Models;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Fragments;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Clients;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Contracts;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Translators;

namespace Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.FragmentProviders;

public class PessoaCoreDataProvider(
    IPersonalDataClient personalDataClient,
    IDadosPessoaisTranslator dadosPessoaisTranslator,
    IExamesMedClient examesMedClient,
    IDadosBiometricosTranslator dadosBiometricosTranslator,
    IIndicacoesTempClient indicacoesTempClient,
    IColocacoesTranslator colocacoesTranslator
) : IPessoaCoreDataProvider
{
    private readonly IPersonalDataClient _personalDataClient = personalDataClient;
    private readonly IDadosPessoaisTranslator _dadosPessoaisTranslator = dadosPessoaisTranslator;
    private readonly IExamesMedClient _examesMedClient = examesMedClient;
    private readonly IDadosBiometricosTranslator _dadosBiometricosTranslator = dadosBiometricosTranslator;
    private readonly IIndicacoesTempClient _indicacoesTempClient = indicacoesTempClient;
    private readonly IColocacoesTranslator _colocacoesTranslator = colocacoesTranslator;

    public async Task<Dictionary<PessoaImportKey, PessoaCoreDataFragment>> GetPessoaCoreDataAsync(IReadOnlyList<PessoaImportKey> importKeys, CancellationToken cancellationToken)
    {
        var personalDataOutputMap = await _personalDataClient.GetPersonalDataAsync(importKeys, cancellationToken);
        var biometricDataOuputMap = await _examesMedClient.GetExamesMedAsync(importKeys, cancellationToken);
        var colocacoesDataOuputMap = await _indicacoesTempClient.GetIndicacoesTempAsync(importKeys, cancellationToken);

        return importKeys.ToDictionary(
            key => key,
            key =>
            {
                var dadosPessoais = _dadosPessoaisTranslator.Translate(personalDataOutputMap[key]);
                var dadosBiometricos = _dadosBiometricosTranslator.Translate(biometricDataOuputMap[key]);
                var colocacoes = _colocacoesTranslator.Translate(colocacoesDataOuputMap[key]);

                return new PessoaCoreDataFragment(dadosPessoais, dadosBiometricos, colocacoes);
            }
        );
    }
}
