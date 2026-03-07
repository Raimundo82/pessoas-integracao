using Pessoas.Integracao.Core.Application.Models;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Fragments;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Clients;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Translators;

namespace Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.FragmentProviders;

public class PessoaCoreDataProvider(IPersonalDataClient personalDataClient, IDadosPessoaisTranslator dadosPessoaisTranslator) : IPessoaCoreDataProvider
{
    private readonly IPersonalDataClient _personalDataClient = personalDataClient;
    private readonly IDadosPessoaisTranslator _dadosPessoaisTranslator = dadosPessoaisTranslator;

    public async Task<Dictionary<PessoaImportKey, PessoaCoreDataFragment>> GetPessoaCoreDataAsync(IReadOnlyList<PessoaImportKey> importKeys, CancellationToken cancellationToken)
    {
        var personalDataOutputMap = await _personalDataClient.GetPersonalDataAsync(importKeys, cancellationToken);
        return personalDataOutputMap.ToDictionary(entryMap => entryMap.Key, entryMap => new PessoaCoreDataFragment(_dadosPessoaisTranslator.Translate(entryMap.Value)));
    }
}