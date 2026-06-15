using System.Collections.Generic;

namespace JADE.models;

public record LangRecord
{
    public string Name { get; init; } = string.Empty;
    public Dictionary<string, string> Texts  { get; init; } = [];
    public Dictionary<string, string> Errors { get; init; } = [];
}