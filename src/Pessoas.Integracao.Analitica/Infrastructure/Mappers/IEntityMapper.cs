namespace Pessoas.Integracao.Analitica.Infrastructure.Mappers;

public interface IEntityMapper<in TSource, out TTarget>
{
    TTarget Map(TSource source, string numsap);
}
