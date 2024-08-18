using Hypercube.Dependencies;
using HypercubeBot.Schemas;
using HypercubeBot.ServiceRealisation;

namespace HypercubeBot.Services;

[Service]
public class TrackingContributors : IStartable
{
    [Dependency] private readonly MongoService _mongoService = default!;
    
    public Task Start()
    {
        foreach (var guild in _mongoService.GetGuilds())
        {
            ProcessGuild(guild.GuildId);
        }
        
        _mongoService.GuildCreated += ProcessGuild;
        
        return Task.CompletedTask;
    }

    private void ProcessGuild(string guildId)
    {
        
    }
    
    private void ProcessGuild(GuildWrapper guild)
    {
        
    }
}