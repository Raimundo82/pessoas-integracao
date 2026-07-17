namespace Pessoas.Integracao.Analitica.Infrastructure.Mappers;

public interface IEntityMapper<TSource, TTarget>
{
    TTarget Map(TSource source, string numsap);
}
