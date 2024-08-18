using Discord.WebSocket;
using Hypercube.Dependencies;
using HypercubeBot.Schemas;
using HypercubeBot.Services;

namespace HypercubeBot.Classes;

public class TrackingGuild
{
    private BotService _botService = default!;
    private GuildWrapper _guildWrapper;
    
    public TrackingGuild(GuildWrapper guildWrapper, BotService botService)
    {
        _guildWrapper = guildWrapper;
        _botService = botService;
    }

    public async Task Start()
    {
        var socket = _botService.Client;
        var trackingRate = int.Parse(Environment.GetEnvironmentVariable("TRAKING_RATE") ?? "10") * 1000;
        var guildId = ulong.Parse(_guildWrapper.Guild.GuildId);
        
        while (true)
        {
            await Task.Delay(trackingRate);
            
            var users = socket.GetGuild(guildId).Users;
            findUserByGithub(users, "1");

            if (users is null)
            {
                continue;
            }
            
            foreach (var (uri, roleId) in _guildWrapper.Guild.Repositories)
            {
                
            }
            
            
        }
        // ReSharper disable once FunctionNeverReturns
    }

    private void findUserByGithub(IReadOnlyCollection<SocketGuildUser> users, string githubUrl)
    {
        
    }
}