using Hypercube.Dependencies;
using HypercubeBot.Classes;
using HypercubeBot.Schemas;
using HypercubeBot.ServiceRealisation;

namespace HypercubeBot.Services;

[Service]
public class TrackingGuilds : IStartable
{
    [Dependency] private readonly MongoService _mongoService = default!;
    [Dependency] private readonly BotService _botService = default!;
    
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
        _ = new TrackingGuild(_mongoService.GetGuild(guildId), _botService).Start();
    }
    
    private void ProcessGuild(GuildWrapper guild)
    {
        _ = new TrackingGuild(guild, _botService).Start();
    }
}