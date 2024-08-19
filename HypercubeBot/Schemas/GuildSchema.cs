using JetBrains.Annotations;

namespace HypercubeBot.Schemas;

[Serializable, Collection("Guilds"), PublicAPI]
public sealed class GuildSchema : Schema, IReadonlyGuild
{
    public Dictionary<string, string> Repositories { get; set; } = new();

    public GuildSchema(string id) : base(id)
    {
    }
}