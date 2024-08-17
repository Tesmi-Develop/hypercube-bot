using MongoDB.Bson;

namespace HypercubeBot.Schemas;

public interface IReadonlyGuild
{
    ObjectId DataId { get; }
    string GuildId { get; }
    List<string> RepositoryUrls { get; }
    string? RoleId { get; }
}