using Discord.Interactions;
using HypercubeBot.Environments;
using JetBrains.Annotations;

namespace HypercubeBot.Commands;

[PublicAPI]
public sealed class GetOauthUrlCommand : InteractionModuleBase
{
    public EnvironmentData EnvironmentData { get; set; } = default!;
    
    [SlashCommand("get_oauth", "Get oauth url")]
    public async Task GetOauthUrl()
    {
        var discordTemplate = $"{EnvironmentData.DiscordApiUri}{EnvironmentData.DiscordApiOauthRoute}";

        var url = Uri.EscapeDataString($"{EnvironmentData.DiscordOauthRedirectHost}{EnvironmentData.DiscordOauthRedirectRoute}");
        var link =
            $"{discordTemplate}?client_id={EnvironmentData.DiscordClientId}&response_type=code&redirect_uri={url}&scope=identify+email+connections";
        
        await RespondAsync(link, ephemeral: true);
    }
}