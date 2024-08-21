using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Hypercube.Dependencies;
using Hypercube.Shared.Logging;
using HypercubeBot.Data;
using HypercubeBot.ServiceRealisation;
using HypercubeBot.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace HypercubeBot.Services;

[Service]
public class BotService : IStartable
{
    public event Action? Connected;
    public bool IsConnected => Client.ConnectionState == ConnectionState.Connected;
    public DiscordSocketClient Client { get; private set; } = default!;

    [Dependency]
    private readonly EnvironmentData _environmentData = default!;
    private readonly Logger _logger = default!;
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
                             GatewayIntents.GuildEmojis,
            AlwaysDownloadUsers = true
        };

        _dependencyWrapper = new DependencyContainerWrapper(DependencyManager.Create());
        Client = new DiscordSocketClient(config);
        
        DependencyManager.Register<IServiceScopeFactory>(_ => new CustomServiceScopeFactory(_dependencyWrapper));
        DependencyManager.Register(Client);
        DependencyManager.Register(x => new InteractionService(x.Resolve<DiscordSocketClient>()));
        
        await Client.LoginAsync(TokenType.Bot, _environmentData.DiscordToken);
        await Client.StartAsync();

        Client.Log += message =>
        {
            _logger.Debug(message.ToString());
            return Task.CompletedTask;
        };
        
        Client.Ready += async () =>
        {
            _commands = new InteractionService(Client);
            
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _dependencyWrapper);

            Client.InteractionCreated += HandleInteraction;

            if (IsDebug())
            {
                var guildId = _environmentData.DiscordDevelopmentGuildId;
                
                await _commands.RegisterCommandsToGuildAsync(ulong.Parse(guildId));
                _logger.Debug($"Commands registered to {guildId}");
            }
            else
            {
                await _commands.RegisterCommandsGloballyAsync();
            }
            Connected?.Invoke();
        };
        
        _logger.Debug("Bot created");
    }
    
    public async Task AwardRole(string userId, string guildId, string roleId)
    {
        var guild = Client.GetGuild(ulong.Parse(guildId));
        if (guild is null)
            throw new ArgumentException("User not found");

        var guildUser = guild.GetUser(ulong.Parse(userId));
        if (guildUser is null)
            throw new ArgumentException("User not found");
        
        await guildUser.AddRoleAsync(ulong.Parse(roleId));
    }
    
    public bool HaveRole(string userId, string guildId, string roleId)
    {
        var guild = Client.GetGuild(ulong.Parse(guildId));
        if (guild is null)
            throw new ArgumentException("User not found");

        var guildUser = guild.GetUser(ulong.Parse(userId));
        if (guildUser is null)
            throw new ArgumentException("User not found");

        var roleIdUlong = ulong.Parse(roleId);
        
        return guildUser.Roles.FirstOrDefault(role => role.Id == roleIdUlong) is not null;
    }
    
    private async Task HandleInteraction (SocketInteraction arg)
    {
        try
        {
            var ctx = new SocketInteractionContext(Client, arg);
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