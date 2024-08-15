using Hypercube.Dependencies;
using HypercubeBot.ServiceRealisation;

namespace HypercubeBot;

public class Program
{
    public static void Main(string[] args)
    {
        DependencyManager.InitThread();
        
        ServiceManager.CreateAll();
        ServiceManager.InitAll();
        ServiceManager.StartAll();

        while (true)
        {
            
        }
    }
}
