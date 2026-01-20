using System.Text.Json.Serialization;

namespace Conflux.Services;

public class IceServerConfiguration {
    [JsonPropertyName("urls")] public string[] Urls { get; set; }
    [JsonPropertyName("username")] public string Username { get; set; }
    [JsonPropertyName("credential")] public string Credential { get; set; } // This is the 'password' in WebRTC terms
}