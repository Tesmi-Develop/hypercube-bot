using Discord;
using Discord.WebSocket;
using Hypercube.Shared.Logging;
using HypercubeBot.ServiceRealisation;

namespace HypercubeBot.Services;

[Service]
public class BotService : IStartable
{
    public event Action? SentMessage;
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

        _client.MessageReceived += message =>
        {
            _logger.Debug($"Message received from {message.Author.Username}: {message.Content}");
            return Task.CompletedTask;
        };

        _client.Ready += () =>
        {
            _logger.Debug("Bot ready");
            initCommand();
            return Task.CompletedTask;
        };

        _client.SlashCommandExecuted += command =>
        {
            command.RespondAsync("Pong!", ephemeral: true);
            return Task.CompletedTask;
        };
        
        _logger.Debug("Bot created");
    }

    private void initCommand()
    {
        var guildCommand = new SlashCommandBuilder();
        guildCommand.WithName("echo");
        guildCommand.WithDescription("Echoes the message");
        
        _client.CreateGlobalApplicationCommandAsync(guildCommand.Build());
    }
}