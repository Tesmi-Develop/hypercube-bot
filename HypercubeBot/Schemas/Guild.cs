using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HypercubeBot.Schemas;

public class Guild(string guildId) : IReadonlyGuild
{
    [BsonId]
    public ObjectId DataId { get; private set; }
    public string GuildId { get; private set; } = guildId;

    public string? RepositoryUrl { get; set; }
    public string? RoleId { get; set; }
}