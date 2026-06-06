namespace Pessoas.Integracao.Worker.Infrastructure.Models.Dados;

public partial class ZhrSDataMedida : ZhrSBaseModel
{
    public int ZhrSAtribOrgOutputId { get; set; }
    public virtual required ZhrSAtribOrgOutput Root { get; set; }
}
