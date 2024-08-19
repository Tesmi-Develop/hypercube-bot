using System.ComponentModel.DataAnnotations;
using Discord;
using Discord.Interactions;
using Discord.Rest;
using HypercubeBot.Schemas;
using HypercubeBot.Services;

namespace HypercubeBot.Commands;

public class GetAllRepositoriesCommand : InteractionModuleBase
{
    public MongoService _MongoService { get; set; }
    
    [SlashCommand("get_all_repositories", "get all repositories"), CommandContextType(InteractionContextType.Guild)]
    public async Task GetAllRepositories()
    {
        await DeferAsync(ephemeral: true);
        
        var message = (RestFollowupMessage)await FollowupAsync("Fetching contributors...", ephemeral: true);
        var guildData = _MongoService.GetData<GuildSchema>(Context.Guild.Id.ToString());
        var repositories = guildData.Data.Repositories;
        var content = "Repositories:\n\n";

        if (guildData.Data.Repositories.Count == 0)
        {
            await message.ModifyAsync(props =>
            {
                props.Content = "No repositories found. You can add one with the `add_repository` command.";
            });
            
            return;
        }
        
        foreach (var (uri, roleId) in repositories)
        {
            content += $"``{uri}`` -<@&{roleId}>\n";
        }
        
        await message.ModifyAsync(props =>
        {
            props.Content = content;
        });
    }
}