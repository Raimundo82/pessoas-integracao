using Pessoas.Integracao.Analitica.Application.Contracts;
using Pessoas.Integracao.Analitica.Infrastructure.Mappers;
using Pessoas.Integracao.Analitica.Models;
using Pessoas.Integracao.Sync.Application.Contracts;
using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;

namespace Pessoas.Integracao.Analitica.Infrastructure.AnaliticaSynchronizer.Synchronizers;

public sealed class PessoaisSyncronizer(
    IEntityMapper<ZhrWsPersonalDataPessoai> mapper,
    IAnaliticaRepository<ZhrWsPersonalDataPessoai> repository)
    : AnaliticaSynchronizerBase<ZhrWsPersonalDataPessoai, ZhrSPessoais>(mapper, repository)
{
    protected override IList<ZhrSPessoais>? GetSourceCollection(IZhrOutput input) => input.Pessoais;
}
