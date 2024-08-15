using Hypercube.Shared.Logging;
using HypercubeBot.ServiceRealisation;

namespace HypercubeBot.Services;

[Service]
public class TestService : IStartable, IInitializable
{
    private readonly Logger _logger = default!;
    
    public async Task Start()
    {
        await Task.Delay(1000);
        Console.WriteLine("Invoked OnStart");
    }

    public void Init()
    {
        Console.WriteLine("Invoked OnInit");
    }

    public void Run()
    {
        _logger.Debug("Invoked Run");
    }
}