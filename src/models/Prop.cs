using System.Data;
using System.Text.Json.Serialization;

namespace JADE.models;

public struct Prop(string key, string value)
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = key ?? throw new NoNullAllowedException();

    [JsonPropertyName("value")]
    public string Value { get; set; } = value ?? throw new NoNullAllowedException();
}