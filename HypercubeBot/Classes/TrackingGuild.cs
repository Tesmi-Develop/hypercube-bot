using Discord.WebSocket;
using Hypercube.Dependencies;
using Hypercube.Shared.Logging;
using HypercubeBot.Environments;
using HypercubeBot.Schemas;
using HypercubeBot.Services;
// ReSharper disable FunctionNeverReturns

namespace HypercubeBot.Classes;

public sealed class TrackingGuild
{
    [Dependency] private readonly EnvironmentData _environmentData = default!;
    [Dependency] private readonly BotService _botService = default!;
    [Dependency] private readonly DiscordUserService _discordUserService = default!;
    [Dependency] private readonly GithubService _githubService = default!;
    private readonly DataWrapper<GuildSchema> _wrapper;
    private readonly Logger _logger = LoggingManager.GetLogger(nameof(TrackingGuild));

    public TrackingGuild(DataWrapper<GuildSchema> wrapper)
    {
        _wrapper = wrapper;
    }

    public void Start()
    {
        if (_botService.IsConnected)
        {
            _ = BotStarted();
            return;
        }
        
        _botService.Connected += () =>
        {
            _ = BotStarted();
        };
    }

    private async Task BotStarted()
    {
        var trackingRate = int.Parse(_environmentData.TrackingRate) * 1000;
        var guild = _botService.Client.GetGuild(ulong.Parse(_wrapper.Data.Id));
        
        while (true)
        {
            await Task.Delay(trackingRate);
            if (_wrapper.Data.Repositories.Count == 0) 
                continue;

            foreach (var user in guild.Users)
            {
                try
                {
                    await ProcessUser(user);
                }
                catch (Exception e)
                {
                    _logger.Error(e.ToString());
                }
            }
        }
    }
    
    private async Task ProcessUser(SocketGuildUser user)
    {
        var userRest = await _discordUserService.TryGetUser(user.Id.ToString());
        if (userRest is null) 
            return;

        var connections = await userRest.GetConnectionsAsync();
        var userGithub = connections.FirstOrDefault(element => element.Type == "github");
        if (userGithub is null) 
            return;

        foreach (var (repository, roleId) in  _wrapper.Data.Repositories)
        {
            await ProcessUserGithub(userGithub.Name, repository, user.Id.ToString(), roleId);
        }
    }
    
    private async Task ProcessUserGithub(string userGithub, string repository, string userId, string roleId)
    {
        if (_botService.HaveRole(userId, _wrapper.Data.Id, roleId)) 
            return;
        
        var contributions = await _githubService.GetContributors(repository);
        if (contributions is null) 
            return;

        if (contributions.Any(contributor => contributor.Login == userGithub))
            await _botService.AwardRole(userId, _wrapper.Data.Id, roleId);
    }
}