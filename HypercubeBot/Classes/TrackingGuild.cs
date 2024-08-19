using System.Runtime.CompilerServices;
using Discord.WebSocket;
using Hypercube.Shared.Logging;
using HypercubeBot.Schemas;
using HypercubeBot.Services;

namespace HypercubeBot.Classes;

public class TrackingGuild
{
    private DataWrapper<GuildSchema> _wrapper;
    private BotService _botService;
    private DiscordUserService _discordUserService;
    private GithubService _githubService;
    private Logger _logger = LoggingManager.GetLogger(nameof(TrackingGuild));

    public TrackingGuild(DataWrapper<GuildSchema> wrapper, BotService botService, DiscordUserService discordUserService,
        GithubService githubService)
    {
        _wrapper = wrapper;
        _botService = botService;
        _discordUserService = discordUserService;
        _githubService = githubService;
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

    public async Task BotStarted()
    {
        var trackingRate = int.Parse(Environment.GetEnvironmentVariable("TRAKING_RATE") ?? "10") * 1000;
        var guild = _botService.Client.GetGuild(ulong.Parse(_wrapper.Data.Id));
        
        while (true)
        {
            await Task.Delay(trackingRate);
            if (_wrapper.Data.Repositories.Count == 0) continue;

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
        // ReSharper disable once FunctionNeverReturns
    }
    
    private async Task ProcessUser(SocketGuildUser user)
    {
        var userRest = await _discordUserService.TryGetUser(user.Id.ToString());
        if (userRest is null) return;

        var connections = await userRest.GetConnectionsAsync();
        var userGithub = connections.FirstOrDefault(element => element.Type == "github");
        if (userGithub is null) return;

        foreach (var (repository, roleId) in  _wrapper.Data.Repositories)
        {
            await ProcessUserGithub(userGithub.Name, repository, user.Id.ToString(), roleId);
        }
    }
    
    private async Task ProcessUserGithub(string userGithub, string repository, string userId, string roleId)
    {
        if (_botService.HaveRole(userId, _wrapper.Data.Id, roleId)) return;
        
        var contributions = await _githubService.GetContributors(repository);
        if (contributions is null) return;

        foreach (var contributor in contributions)
        {
            //if (contributor.Login != userGithub) continue;
         
            await _botService.AwardRole(userId, _wrapper.Data.Id, roleId);
            break;
        }
    }
}