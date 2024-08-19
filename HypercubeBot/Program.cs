using dotenv.net;
using Hypercube.Dependencies;
using HypercubeBot.ServiceRealisation;

namespace HypercubeBot;

public class Program
{
    public static void LoadEvn(string filePath)
    {
        if (!File.Exists(filePath))
            return;

        foreach (var line in File.ReadAllLines(filePath))
        {
            var parts = line.Split(
                '=',
                StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != 2)
                continue;

            Environment.SetEnvironmentVariable(parts[0], parts[1]);
        }
    }
    
    public static async Task Main(string[] args)
    {
        LoadEvn(".env");
        DependencyManager.InitThread();
        
        ServiceManager.CreateAll();
        ServiceManager.InitAll();
        ServiceManager.StartAll();

        await Task.Delay(-1);
    }
}
