using Conflux.Web.Core;
using System.Text;
using System.Text.Json.Serialization;

namespace Conflux.Web.Services;

internal sealed class CloudflareTurnServerClient(HttpClient httpClient, IConfiguration configuration) {
    public async Task<IceServerConfiguration[]> GenerateIceServerConfigurations() {
        using var request = new HttpRequestMessage();
        request.RequestUri = new($"https://rtc.live.cloudflare.com/v1/turn/keys/{configuration["Cloudflare:TurnServer:TokenID"]}/credentials/generate-ice-servers");
        request.Method = HttpMethod.Post;
        request.Headers.Authorization = new("Bearer", configuration["Cloudflare:TurnServer:ApiToken"]);
        request.Content = new StringContent("{\"ttl\": 86400}", Encoding.UTF8, "application/json");

        try {
            var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadFromJsonAsync<CloudflareIceServerConfigurationResult>();
            return content?.IceServers ?? [];
        } catch (HttpRequestException e) {
            return [];
        }
    }
}

file sealed class CloudflareIceServerConfigurationResult {
    [JsonPropertyName("iceServers")] public IceServerConfiguration[] IceServers { get; set; } = null!;
}