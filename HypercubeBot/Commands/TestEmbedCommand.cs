using Discord;
using Discord.Interactions;

namespace HypercubeBot.Commands;

public class TestEmbedCommand : InteractionModuleBase
{
    [SlashCommand("test_embed", "Test embed")]
    public async Task TestEmbed()
    {
        var embed = new EmbedBuilder();
        // Or with methods
        embed.AddField("Field title",
                "Field value. I also support [hyperlink markdown](https://example.com)!")
            .WithAuthor(Context.Client.CurrentUser)
            .WithFooter(footer => footer.Text = "I am a footer.")
            .WithColor(Color.Blue)
            .WithTitle("I overwrote \"Hello world!\"")
            .WithDescription("I am a description.")
            .WithUrl("https://example.com")
            .WithCurrentTimestamp();
        
        //Your embed needs to be built before it is able to be sent
        await RespondAsync(embed: embed.Build(), ephemeral: true);
    } 
}