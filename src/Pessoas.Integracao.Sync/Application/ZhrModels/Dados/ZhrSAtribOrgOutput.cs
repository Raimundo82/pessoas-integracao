namespace Pessoas.Integracao.Sync.Application.ZhrModels.Dados;

public partial class ZhrSAtribOrgOutput : ZhrSBaseModelOutput, IOutputModel
{
    public override IReadOnlyList<ZhrSBaseModel> GetChildrenFlattened()
    {
        return
        [
            ..AtribOrg ?? [],
            ..MonitPrazos ?? [],
            ..DataMedida ?? [],
            ..Om ?? [],
            ..ClassifProf ?? []
        ];
    }
}

public partial class ZhrSAtribOrg : ZhrSBaseModel { }
public partial class ZhrSMonitPrazos : ZhrSBaseModel { }
public partial class ZhrSDataMedida : ZhrSBaseModel { }
public partial class ZhrSOm : ZhrSBaseModel { }
public partial class ZhrSClassifProf : ZhrSBaseModel { }

