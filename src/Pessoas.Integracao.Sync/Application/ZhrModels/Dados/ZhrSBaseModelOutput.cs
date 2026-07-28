using System.Xml.Serialization;

namespace Pessoas.Integracao.Sync.Application.ZhrModels.Dados;

public interface IOutputModel
{
    string Ni { get; set; }
    string Numsap { get; set; }
    IReadOnlyList<ZhrSBaseModel> GetChildrenFlattened();
    void SetChildrenNi()
    {
        foreach (var child in GetChildrenFlattened())
        {
            child.Ni = Ni;
        }
    }
}

public abstract class ZhrSBaseModelOutput
{
    [XmlElement(Order = 0)]
    public DateTimeOffset? UpdatedAt { get; set; }


    public virtual void SetUpdatedAt(DateTimeOffset updatedAt)
    {
        UpdatedAt = updatedAt;
    }
}
