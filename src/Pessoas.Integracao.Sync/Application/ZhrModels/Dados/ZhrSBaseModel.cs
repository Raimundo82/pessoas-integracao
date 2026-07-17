using System.Xml.Serialization;

namespace Pessoas.Integracao.Sync.Application.ZhrModels.Dados;

public abstract class ZhrSBaseModel
{
    [XmlElement(Order = 0)]
    public int Id { get; set; }

    [XmlElement(Order = 1)]
    public required string Ni { get; set; }

}
