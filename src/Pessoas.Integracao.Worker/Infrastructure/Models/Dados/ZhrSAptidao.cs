namespace Pessoas.Integracao.Worker.Infrastructure.Models.Dados;

public partial class ZhrSAptidao : ZhrWsBaseModel
{
    public int ZhrSAptidaoOutputId { get; set; }
    public required ZhrSAptidaoOutput Output { get; set; }
}

