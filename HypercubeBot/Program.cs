using System.Diagnostics;
using dotenv.net;
using Hypercube.Dependencies;
using HypercubeBot.ServiceRealisation;

namespace HypercubeBot;

public class Program
{
    private static bool _running;
    
    public static Task Main(string[] args)
    {
        DotEnv.Load();
        DependencyManager.InitThread();
        
        ServiceManager.CreateAll();
        ServiceManager.InitAll();
        ServiceManager.StartAll();

        _running = true;

        while (_running)
        {
            Thread.Sleep(10);
        }

        return Task.CompletedTask;
    }
    
    public static void Shutdown()
    {
        _running = false;
    }
}
