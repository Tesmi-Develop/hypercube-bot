using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using HypercubeBot.Schemas;
using HypercubeBot.Services;
// ReSharper disable MemberCanBePrivate.Global

namespace HypercubeBot.Commands;

public sealed class AddRepositoryCommand : InteractionModuleBase
{
    public GithubService GithubService { get; set; } = default!;
    public BotService BotService { get; set; } = default!;
    public MongoService MongoService { get; set; } = default!;
    
    [SlashCommand("add_repository", "add a repository"), CommandContextType(InteractionContextType.Guild)]
    public async Task AddRepository(string repositoryUrl, IRole role)
    {
        await DeferAsync(ephemeral: true);
        var message = (RestFollowupMessage)await FollowupAsync("Fetching contributors...", ephemeral: true);
        var guildData = MongoService.GetData<GuildSchema>(Context.Guild.Id.ToString());
        
        if (guildData.Data.Repositories.ContainsKey(repositoryUrl))
        {
            await message.ModifyAsync(props =>
            {
                props.Content = "Repository already exists";
            });
            return;
        }
        
        var myRole = GetHightestMyRole(Context.Guild.Id);
        if (myRole is null || role.Position >= myRole.Position)
        {
            await message.ModifyAsync(props =>
            {
                props.Content = "I can't give away that role";
            });
            return;
        }
        
        var contributors = await GithubService.GetContributors(repositoryUrl);
        
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

    private SocketRole? GetHightestMyRole(ulong guildId)
    {
        var guild = BotService.Client.Guilds.First(guild => guild.Id == guildId)!;
        var roles = guild.Users.First(user => user.Id == BotService.Client.CurrentUser.Id).Roles;
        var maxPosition = -1;
        SocketRole? pickedRole = null;

        foreach (var role in roles)
        {
            if (role.Position <= maxPosition) continue;
            maxPosition = role.Position;
            pickedRole = role;
        }

        return pickedRole;
    }
}