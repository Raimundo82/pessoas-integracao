using Pessoas.Integracao.Analitica.Models;
using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;

using Riok.Mapperly.Abstractions;

namespace Pessoas.Integracao.Analitica.Infrastructure.Mappers;

/// <summary>
/// Shared mapping configurations for Riok.Mapperly across multiple mappers.
/// This class provides common mapping rules that can be included in other mappers
/// using the [IncludeMappingConfiguration] attribute.
/// </summary>
/// <example>
/// <code>
/// [IncludeMappingConfiguration(nameof(SharedMappingConfig.IgnoreCommonFields))]
/// private partial TargetMap MapFields(Source source);
/// </code>
/// </example>
[Mapper]
internal static partial class SharedMappingConfig
{
    /// <summary>
    /// Ignores common fields that should not be mapped from source to target:
    /// - Source Id: The source model's Id is not relevant for the target model
    /// - Target Id: The target model's Id is managed by the database
    /// - Target UpdatedAt: The target model's UpdatedAt is set during synchronization
    /// </summary>
    [MapperIgnoreSource(nameof(ZhrSBaseModel.Id))]
    [MapperIgnoreTarget(nameof(IAnaliticaModel.Id))]
    [MapperIgnoreTarget(nameof(IAnaliticaModel.UpdatedAt))]
    public static partial void IgnoreCommonFields(ZhrSBaseModel source, AnaliticaBaseModel target);
}
