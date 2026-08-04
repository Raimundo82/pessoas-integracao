using Pessoas.Integracao.Analitica.Application.Contracts;
using Pessoas.Integracao.Analitica.Infrastructure.Transformers;
using Pessoas.Integracao.Analitica.Models;
using Pessoas.Integracao.Sync.Application.Contracts;
using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;

namespace Pessoas.Integracao.Analitica.Infrastructure.AnaliticaSynchronizer.Synchronizers;

public sealed class AptidaoSynchronizer(
    IDataTransformer<ZhrWsAptidaoAptidao, ZhrSAptidao> transformer,
    IAnaliticaRepository<ZhrWsAptidaoAptidao> repository)
    : BaseSynchronizer<ZhrWsAptidaoAptidao, ZhrSAptidao>(transformer, repository)
{
    protected override IList<ZhrSAptidao>? GetZhrOutputSlice(IZhrOutput output)
        => output.Aptidoes;
}
