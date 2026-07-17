using System.Xml.Serialization;

namespace Pessoas.Integracao.Sync.Application.ZhrModels.Dados;

public interface IOutputModel
{
    string Ni { get; set; }
    string Numsap { get; set; }
}

public abstract class ZhrSBaseModelOutput
{
    [XmlElement(Order = 0)]
    public DateTimeOffset? UpdatedAt { get; set; }

}
