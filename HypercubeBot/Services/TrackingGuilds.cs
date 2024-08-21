using Hypercube.Dependencies;
using HypercubeBot.Classes;
using HypercubeBot.Schemas;
using HypercubeBot.ServiceRealisation;

namespace HypercubeBot.Services;

[Service]
public class TrackingGuilds : IStartable
{
    [Dependency] private readonly MongoService _mongoService = default!;
    
    public Task Start()
    {
        foreach (var wrapper in _mongoService.GetData<GuildSchema>())
        {
            ProcessGuild(wrapper);
        }

        _mongoService.SchemaAdded += schema =>
        {
            if (schema is not DataWrapper<GuildSchema> wrapper)
                return;
            
            ProcessGuild(wrapper);
        };
        
        return Task.CompletedTask;
    }

    private static void ProcessGuild(DataWrapper<GuildSchema> wrapper)
    {
        var tracking = new TrackingGuild(wrapper);
        DependencyManager.Inject(tracking);
        
        tracking.Start();
    }
}