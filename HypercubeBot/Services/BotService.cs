using Discord;
using Discord.WebSocket;
using Hypercube.Shared.Logging;
using HypercubeBot.ServiceRealisation;

namespace HypercubeBot.Services;

[Service]
public class BotService : IStartable
{
    private readonly Logger _logger = default!;
    private DiscordSocketClient _client = default!;
    
    public async Task Start()
    {
        var config = new DiscordSocketConfig()
        {
            GatewayIntents = GatewayIntents.Guilds |
                             GatewayIntents.MessageContent |
                             GatewayIntents.GuildMembers |
                             GatewayIntents.Guilds |
                             GatewayIntents.GuildMessages | GatewayIntents.GuildMessageReactions |
                             GatewayIntents.GuildBans |
                             GatewayIntents.GuildEmojis
        };
        
        _client = new DiscordSocketClient(config);
        await _client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("TOKEN"));
        await _client.StartAsync();

        _client.Log += message =>
        {
            _logger.Debug(message.ToString());
            return Task.CompletedTask;
        };

        _client.Ready += () =>
        {
            _logger.Debug("Bot ready");
            return Task.CompletedTask;
        };
        
        _logger.Debug("Bot created");
    }
}