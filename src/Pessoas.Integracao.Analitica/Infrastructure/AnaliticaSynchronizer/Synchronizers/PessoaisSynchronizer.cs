using Pessoas.Integracao.Analitica.Application.Contracts;
using Pessoas.Integracao.Analitica.Infrastructure.Transformers;
using Pessoas.Integracao.Analitica.Models;
using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;

namespace Pessoas.Integracao.Analitica.Infrastructure.AnaliticaSynchronizer.Synchronizers;

public sealed class PessoaisSynchronizer(
    IDataTransformer<ZhrWsPersonalDataPessoai, ZhrSPessoais> transformer,
    IAnaliticaRepository<ZhrWsPersonalDataPessoai> repository)
    : BaseSynchronizer<ZhrWsPersonalDataPessoai, ZhrSPessoais>(transformer, repository)
{
}
