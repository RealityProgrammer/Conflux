using System.Text.Json.Serialization;

namespace Conflux.Web.Core;

public class IceServerConfiguration {
    [JsonPropertyName("urls")] public string[] Urls { get; set; } = null!;
    [JsonPropertyName("username")] public string Username { get; set; }= null!;
    [JsonPropertyName("credential")] public string Credential { get; set; } = null!;
}