using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.Rest;
using HypercubeBot.Schemas;
using HypercubeBot.Services;

namespace HypercubeBot.Commands;

public class AddRepositoryCommand : InteractionModuleBase
{
    public GithubService _GithubService { get; set; }
    public MongoService _MongoService { get; set; }
    
    [SlashCommand("add_repository", "add a repository"), CommandContextType(InteractionContextType.Guild)]
    public async Task AddRepository(string repositoryUrl, IRole role)
    {
        await DeferAsync(ephemeral: true);
        var message = (RestFollowupMessage)await FollowupAsync("Fetching contributors...", ephemeral: true);
        var guildData = _MongoService.GetData<GuildSchema>(Context.Guild.Id.ToString());
        
        if (guildData.Data.Repositories.ContainsKey(repositoryUrl))
        {
            await message.ModifyAsync(props =>
            {
                props.Content = "Repository already exists";
            });
            return;
        }
        
        var contributors = await _GithubService.GetContributors(repositoryUrl);
        
        if (contributors is null)
        {
            await message.ModifyAsync(props =>
            {
                props.Content = "Invalid repository url";
            });
            return;
        }
        
        guildData.Mutate(draft =>
        {
            draft.Repositories[repositoryUrl] = role.Id.ToString();
        });
        
        await message.ModifyAsync(props =>
        {
            props.Content = "Done!";
        });
    }
}