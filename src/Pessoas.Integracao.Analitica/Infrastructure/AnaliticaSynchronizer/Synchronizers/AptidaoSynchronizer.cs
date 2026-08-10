using Pessoas.Integracao.Analitica.Application.Contracts;
using Pessoas.Integracao.Analitica.Infrastructure.Transformers;
using Pessoas.Integracao.Analitica.Models;

namespace Pessoas.Integracao.Analitica.Infrastructure.AnaliticaSynchronizer.Synchronizers;

public sealed class AptidaoSynchronizer(
    IDataTransformer<ZhrWsAptidaoAptidao> transformer,
    IAnaliticaRepository<ZhrWsAptidaoAptidao> repository)
    : BaseSynchronizer<ZhrWsAptidaoAptidao>(transformer, repository)
{
}
