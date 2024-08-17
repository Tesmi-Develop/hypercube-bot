using System.Diagnostics;
using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Hypercube.Dependencies;
using Hypercube.Shared.Logging;
using HypercubeBot.ServiceRealisation;
using HypercubeBot.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace HypercubeBot.Services;

[Service]
public class BotService : IStartable
{
    private readonly Logger _logger = default!;
    private DiscordSocketClient _client = default!;
    private InteractionService _commands = default!;
    private DependencyContainerWrapper _dependencyWrapper = default!;
    
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

        _dependencyWrapper = new(DependencyManager.GetContainer());
        _client = new DiscordSocketClient(config);
        
        DependencyManager.Register<IServiceScopeFactory>(_ => new CustomServiceScropeFactory(_dependencyWrapper));
        DependencyManager.Register(_client);
        DependencyManager.Register(x => new InteractionService(x.Resolve<DiscordSocketClient>()));
        
        var token = Environment.GetEnvironmentVariable("TOKEN");
        Debug.Assert(token != null, "Missing TOKEN environment variable");
        
        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

        _client.Log += message =>
        {
            _logger.Debug(message.ToString());
            return Task.CompletedTask;
        };
        
        _client.Ready += async () =>
        {
            _commands = new InteractionService(_client);
            Console.WriteLine(_dependencyWrapper.GetService(typeof(BotService)));
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _dependencyWrapper);

            _client.InteractionCreated += HandleInteraction;

            if (IsDebug())
            {
                var guildId = Environment.GetEnvironmentVariable("TEST_GUILD");
                Debug.Assert(guildId != null, "Missing TEST_GUILD environment variable");
                
                await _commands.RegisterCommandsToGuildAsync(ulong.Parse(guildId));
                _logger.Debug($"Commands registered to {guildId}");
            }
            else
            {
                await _commands.RegisterCommandsGloballyAsync();
            }
        };
        
        _logger.Debug("Bot created");
    }
    
    private async Task HandleInteraction (SocketInteraction arg)
    {
        try
        {
            var ctx = new SocketInteractionContext(_client, arg);
            DependencyManager.InitThread();
            
            await _commands.ExecuteCommandAsync(ctx, _dependencyWrapper);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
           
            if(arg.Type == InteractionType.ApplicationCommand)
            {
                await arg.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
            }
        }
    }

    private static bool IsDebug( )
    {
        #if DEBUG
            return true;
        #else
            return false;
        #endif
    }
}