﻿using MongoDB.Bson;

namespace HypercubeBot.Schemas;

public interface IReadonlyGuild
{
    Dictionary<string, string> Repositories { get; }
}