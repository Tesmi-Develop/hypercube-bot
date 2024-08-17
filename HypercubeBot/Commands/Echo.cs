using Discord.Interactions;
using Hypercube.Dependencies;
using HypercubeBot.Services;

namespace HypercubeBot.Commands;

public class EchoCommand : InteractionModuleBase
{
    public BotService _bot { get; set; }
    
    [SlashCommand("echo", "echoes whatever you say1")]
    public async Task Echo(string input)
    {
        Console.WriteLine(_bot);
        await RespondAsync(input, ephemeral: true);
    }
}