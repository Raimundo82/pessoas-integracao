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


    public virtual void SetUpdatedAt(DateTimeOffset updatedAt)
    {
        UpdatedAt = updatedAt;
    }

    public abstract IReadOnlyList<ZhrSBaseModel> GetChildren();

    public void SetNi(string ni)
    {
        foreach (var child in GetChildren().Where(c => string.IsNullOrWhiteSpace(c.Ni)))
        {
            child.Ni = ni;
        }
    }
}
