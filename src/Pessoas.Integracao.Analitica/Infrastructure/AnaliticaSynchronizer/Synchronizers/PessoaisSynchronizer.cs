using Pessoas.Integracao.Analitica.Application.Contracts;
using Pessoas.Integracao.Analitica.Infrastructure.Transformers;
using Pessoas.Integracao.Analitica.Models;

namespace Pessoas.Integracao.Analitica.Infrastructure.AnaliticaSynchronizer.Synchronizers;

public sealed class PessoaisSynchronizer(
    IDataTransformer<ZhrWsPersonalDataPessoai> transformer,
    IAnaliticaRepository<ZhrWsPersonalDataPessoai> repository)
    : BaseSynchronizer<ZhrWsPersonalDataPessoai>(transformer, repository)
{
}
