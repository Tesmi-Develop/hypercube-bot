using MongoDB.Bson;

namespace HypercubeBot.Schemas;

public interface IReadonlyGuild
{
    ObjectId DataId { get; }
    string GuildId { get; }
    string? RepositoryUrl { get; }
    string? RoleId { get; }
}