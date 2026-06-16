using System.ServiceModel.Channels;

namespace Pessoas.Integracao.Sync.Infrastructure.Factories;

public interface IBindingFactory
{
    Binding CreateBinding();
}
