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
    [Dependency] private readonly DiscordUserService _discordUserService = default!;
    [Dependency] private readonly GithubService _githubService = default!;
    
    public Task Start()
    {
        foreach (var wrapper in _mongoService.GetData<GuildSchema>())
        {
            ProcessGuild(wrapper);
        }

        _mongoService.SchemaAdded += schema =>
        {
            if (schema is not DataWrapper<GuildSchema> wrapper) return;
            ProcessGuild(wrapper);
        };
        
        return Task.CompletedTask;
    }

    private void ProcessGuild(DataWrapper<GuildSchema> wrapper)
    {
        new TrackingGuild(wrapper, _botService, _discordUserService, _githubService).Start();
    }
}