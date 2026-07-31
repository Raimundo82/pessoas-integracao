using Pessoas.Integracao.Analitica.Application.Contracts;
using Pessoas.Integracao.Analitica.Infrastructure.Mappers;
using Pessoas.Integracao.Analitica.Models;
using Pessoas.Integracao.Sync.Application.Contracts;
using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;

namespace Pessoas.Integracao.Analitica.Infrastructure.AnaliticaSynchronizer.Synchronizers;

public sealed class AptidaoSynchronizer(
    IEntityMapper<ZhrWsAptidaoAptidao> mapper,
    IAnaliticaRepository<ZhrWsAptidaoAptidao> repository)
    : AnaliticaSynchronizerBase<ZhrWsAptidaoAptidao, ZhrSAptidao>(mapper, repository)
{
    protected override IList<ZhrSAptidao>? GetZhrOutputSlice(IZhrOutput input) => input.Aptidoes;
}
