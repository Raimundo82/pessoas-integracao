using Pessoas.Integracao.Analitica.Infrastructure.Mappers;
using Pessoas.Integracao.Analitica.Models;
using Pessoas.Integracao.Sync.Application.Contracts;
using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;

namespace Pessoas.Integracao.Analitica.Infrastructure.Transformers;

public sealed class PessoaisDataTransformer(
    IEntityMapper<ZhrWsPersonalDataPessoai> mapper)
    : BaseDataTransformer<ZhrWsPersonalDataPessoai, ZhrSPessoais>(mapper)
{
    protected override IList<ZhrSPessoais>? GetZhrOutputSlice(IZhrOutput input)
        => input.Pessoais;
}
