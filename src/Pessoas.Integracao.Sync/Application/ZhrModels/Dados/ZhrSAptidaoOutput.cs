namespace Pessoas.Integracao.Sync.Application.ZhrModels.Dados;

public partial class ZhrSAptidaoOutput : ZhrSBaseModelOutput, IOutputModel
{
    public override IReadOnlyList<ZhrSBaseModel> GetChildrenFlattened()
            => Aptidao?.Cast<ZhrSBaseModel>().ToArray() ?? [];
}

public partial class ZhrSAptidao : ZhrSBaseModel { }
