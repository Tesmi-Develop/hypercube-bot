using Hypercube.Dependencies;
using HypercubeBot.ServiceRealisation;

namespace HypercubeBot;

public class Program
{
    public static async Task Main(string[] args)
    {
        DependencyManager.InitThread();
        
        ServiceManager.CreateAll();
        ServiceManager.InitAll();
        ServiceManager.StartAll();

        await Task.Delay(-1);
    }
}
