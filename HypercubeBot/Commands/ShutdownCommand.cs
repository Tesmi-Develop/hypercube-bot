using Discord.Interactions;
using JetBrains.Annotations;

namespace HypercubeBot.Commands;

[PublicAPI]
public sealed class ShutdownCommand : InteractionModuleBase
{
    [SlashCommand("shutdown", "Shuts down the bot")]
    public async Task Shutdown()
    {
        await RespondAsync("Bot is shutting down...", ephemeral: true);
        Program.Shutdown();
    }
}