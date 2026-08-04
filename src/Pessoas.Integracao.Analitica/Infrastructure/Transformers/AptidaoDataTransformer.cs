using Pessoas.Integracao.Analitica.Infrastructure.Mappers;
using Pessoas.Integracao.Analitica.Models;
using Pessoas.Integracao.Sync.Application.Contracts;
using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;

namespace Pessoas.Integracao.Analitica.Infrastructure.Transformers;

public sealed class AptidaoDataTransformer(
    IEntityMapper<ZhrWsAptidaoAptidao> mapper
) : BaseDataTransformer<ZhrWsAptidaoAptidao, ZhrSAptidao>(mapper)
{
    protected override IList<ZhrSAptidao>? GetZhrOutputSlice(IZhrOutput output)
        => output.Aptidoes;
}
