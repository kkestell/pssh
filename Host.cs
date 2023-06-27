using System.Text.Json.Serialization;

namespace Pssh;

public class Host
{
    [JsonPropertyName("address")]
    public string Address { get; set; }

    [JsonPropertyName("color")]
    public string Color { get; set; }

    [JsonPropertyName("username")]
    public string Username { get; set; }

    [JsonPropertyName("port")]
    public int Port { get; set; }

    [JsonPropertyName("key_file")]
    public string KeyFile { get; set; }
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(Host))]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(List<Host>))]
internal partial class SourceGenerationContext : JsonSerializerContext
{
}