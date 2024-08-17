using Discord.Interactions;

namespace HypercubeBot.Commands;

public class AddRepositoryCommand : InteractionModuleBase
{
    
    [SlashCommand("add-repository", "add a repository")]
    public async Task AddRepository(string repositoryUrl)
    {
        
    }
}