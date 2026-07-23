namespace Pessoas.Integracao.Sync.Application.ZhrModels.Dados;

public partial class ZhrSPessoaisOutput : ZhrSBaseModelOutput, IOutputModel
{
    public override IReadOnlyList<ZhrSBaseModel> GetChildrenFlattened()
    {
        return
        [
            ..Pessoais ?? [],
            ..Familia ?? [],
            ..OutrosDados ?? [],
            ..Deficiencias ?? []
        ];
    }
}

public partial class ZhrSPessoais : ZhrSBaseModel { }

public partial class ZhrSFamilia : ZhrSBaseModel { }

public partial class ZhrSOutrosdados : ZhrSBaseModel { }

public partial class ZhrSDeficiencias : ZhrSBaseModel { }
