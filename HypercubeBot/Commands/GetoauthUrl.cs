using System.Diagnostics;
using Discord.Interactions;
using HypercubeBot.Data;
using HypercubeBot.Services;
// ReSharper disable MemberCanBePrivate.Global

namespace HypercubeBot.Commands;

public class GetOauthUrlCommand : InteractionModuleBase
{
    public OauthService OauthService { get; set; } = default!;
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