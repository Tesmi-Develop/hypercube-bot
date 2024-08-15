using dotenv.net;
using HypercubeBot.ServiceRealisation;

namespace HypercubeBot.Services;

[Service]
public class EnvService : IInitializable
{
    
    public void Init()
    {
        DotEnv.Load();
    }
}