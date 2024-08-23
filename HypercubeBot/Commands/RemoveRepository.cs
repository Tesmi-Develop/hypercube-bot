using Discord;
using Discord.Interactions;
using Discord.Rest;
using HypercubeBot.Embeds;
using HypercubeBot.Schemas;
using HypercubeBot.Services;
// ReSharper disable once MemberCanBePrivate.Global

namespace HypercubeBot.Commands;

public class RemoveRepositoryCommand : InteractionModuleBase
{
    public MongoService MongoService { get; set; } = default!;
    
    [SlashCommand("remove_repository", "remove a repository"), CommandContextType(InteractionContextType.Guild)]
    public async Task RemoveRepository(string repositoryUrl)
    {
        await DeferAsync(ephemeral: true);
        var embed = new MessageWithEmbed(description: "Processing...");
        var message = (RestFollowupMessage) await FollowupAsync(ephemeral: true, embed: embed.Embed.Build());
        embed.BindMessage(message);
        
        var guildData = MongoService.GetData<GuildSchema>(Context.Guild.Id.ToString());

        if (!guildData.Data.Repositories.ContainsKey(repositoryUrl))
        {
            await embed.SetDescription("Repository not found");
            return;
        }
        
        guildData.Mutate(draft =>
        {
            draft.Repositories.Remove(repositoryUrl);
        });
        
        await embed.SetDescription("Done!");
    }
}