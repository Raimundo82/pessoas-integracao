namespace Pessoas.Integracao.Worker.Infrastructure.Models.Dados;

public partial class ZhrSMobilidadesOutput : ZhrSBaseModelOutput { }

public partial class ZhrSMobilidades : ZhrSBaseModel
{
    public int ZhrSMobilidadesOutputId { get; set; }
    public virtual required ZhrSMobilidadesOutput Root { get; set; }
}

