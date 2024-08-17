using Hypercube.Dependencies;

namespace HypercubeBot.Utils;

public class DependencyContainerWrapper(DependenciesContainer container) : IServiceProvider
{
    public object? GetService(Type serviceType)
    {
        return container.Resolve(serviceType);
    }
}