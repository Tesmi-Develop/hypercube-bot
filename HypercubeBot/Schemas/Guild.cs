using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HypercubeBot.Schemas;

public class Guild(string guildId) : IReadonlyGuild
{
    [BsonId]
    public ObjectId DataId { get; private set; }
    public string GuildId { get; private set; } = guildId;

    public List<string> RepositoryUrls { get; set; } = new();
    public string? RoleId { get; set; }
}