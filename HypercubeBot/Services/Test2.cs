using Hypercube.Dependencies;
using HypercubeBot.ServiceRealisation;

namespace HypercubeBot.Services;

[Service]
public class Test2 : IInitializable
{
    [Dependency] private readonly TestService _test = default!;

    public void Init()
    {
        _test.Run();
    }
}