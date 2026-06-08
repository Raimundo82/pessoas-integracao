namespace Pessoas.Integracao.Worker.Infrastructure.Models.Dados;

public partial class ZhrSAtribOrgOutput : ZhrSBaseModelOutput { }

public partial class ZhrSAtribOrg : ZhrSBaseModel
{
    public int ZhrSAtribOrgOutputId { get; set; }
    public virtual required ZhrSAtribOrgOutput Root { get; set; }
}

public partial class ZhrSClassifProf : ZhrSBaseModel
{
    public int ZhrSAtribOrgOutputId { get; set; }
    public virtual required ZhrSAtribOrgOutput Root { get; set; }
}

public partial class ZhrSDataMedida : ZhrSBaseModel
{
    public int ZhrSAtribOrgOutputId { get; set; }
    public virtual required ZhrSAtribOrgOutput Root { get; set; }
}

public partial class ZhrSInfoProm : ZhrSBaseModel
{
    public int ZhrSAtribOrgOutputId { get; set; }
    public virtual required ZhrSAtribOrgOutput Root { get; set; }
}

public partial class ZhrSMonitPrazos : ZhrSBaseModel
{
    public int ZhrSAtribOrgOutputId { get; set; }
    public virtual required ZhrSAtribOrgOutput Root { get; set; }
}

public partial class ZhrSOm : ZhrSBaseModel
{
    public int ZhrSAtribOrgOutputId { get; set; }
    public virtual required ZhrSAtribOrgOutput Root { get; set; }
}
