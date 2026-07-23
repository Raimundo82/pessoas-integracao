namespace Pessoas.Integracao.Sync.Application.ZhrModels.Dados;

public partial class ZhrSPessoaisOutput : ZhrSBaseModelOutput, IOutputModel
{
    public override IReadOnlyList<ZhrSBaseModel> GetChildrenFlattened()
    {
        return
        [
            ..Pessoais?.Cast<ZhrSBaseModel>() ?? [],
            ..Familia?.Cast<ZhrSBaseModel>() ?? [],
            ..OutrosDados?.Cast<ZhrSBaseModel>() ?? [],
            ..Deficiencias?.Cast<ZhrSBaseModel>() ?? []
        ];
    }
}

public partial class ZhrSPessoais : ZhrSBaseModel { }

public partial class ZhrSFamilia : ZhrSBaseModel { }

public partial class ZhrSOutrosdados : ZhrSBaseModel { }

public partial class ZhrSDeficiencias : ZhrSBaseModel { }
