namespace Pessoas.Integracao.Sync.Application.ZhrModels.Dados;

public partial class ZhrSAptidaoOutput : ZhrSBaseModelOutput, IOutputModel
{
    public override IReadOnlyList<ZhrSBaseModel> GetChildrenFlattened()
            => Aptidao ?? [];
}

public partial class ZhrSAptidao : ZhrSBaseModel { }
