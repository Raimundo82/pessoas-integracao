namespace Pessoas.Integracao.Sync.Application.ZhrModels.Dados;

public partial class ZhrSAptidaoOutput : ZhrSBaseModelOutput, IOutputModel
{
    public override IReadOnlyList<ZhrSBaseModel> GetChildren()
            => Aptidao?.Cast<ZhrSBaseModel>().ToArray() ?? [];
}

public partial class ZhrSAptidao : ZhrSBaseModel { }
