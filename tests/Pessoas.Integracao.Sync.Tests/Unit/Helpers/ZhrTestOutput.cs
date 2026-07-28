using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;

namespace Pessoas.Integracao.Sync.Tests.Unit.Helpers;

public sealed class ZhrTestOutput : ZhrSBaseModelOutput, IOutputModel
{
    public IReadOnlyList<ZhrSBaseModel> Children { get; init; } = [];
    public required string Ni { get; set; }
    public required string Numsap { get; set; }
    public IReadOnlyList<ZhrSBaseModel> GetChildrenFlattened() => Children;
}

public sealed class ZhrChildA : ZhrSBaseModel { }

public sealed class ZhrChildB : ZhrSBaseModel { }
