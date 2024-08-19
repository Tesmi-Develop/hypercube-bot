using Discord;
using Discord.Interactions;
using Discord.Rest;
using HypercubeBot.Schemas;
using HypercubeBot.Services;

namespace HypercubeBot.Commands;

public class RemoveRepositoryCommand : InteractionModuleBase
{
    public MongoService MongoService { get; set; } = default!;
    
    [SlashCommand("remove_repository", "remove a repository"), CommandContextType(InteractionContextType.Guild)]
    public async Task RemoveRepository(string repositoryUrl)
    {
        await DeferAsync(ephemeral: true);
        var message = (RestFollowupMessage)await FollowupAsync("Processing...", ephemeral: true);
        
        var guildData = MongoService.GetData<GuildSchema>(Context.Guild.Id.ToString());

        if (!guildData.Data.Repositories.ContainsKey(repositoryUrl))
        {
            await message.ModifyAsync(props =>
            {
                props.Content = "Repository not found";
            });
            return;
        }
        
        guildData.Mutate(draft =>
        {
            draft.Repositories.Remove(repositoryUrl);
        });
        
        await message.ModifyAsync(props =>
        {
            props.Content = "Done!";
        });
    }
}