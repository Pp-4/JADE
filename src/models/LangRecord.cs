using System.Collections.Generic;

namespace JADE.models;

public record LangRecord
{
    public string Name { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public Dictionary<string, string> SystemMessages  { get; init; } = [];
    public Dictionary<string, string> UserMessages { get; init; } = [];
}