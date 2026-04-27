using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace MonthioSample;

public class MonthioAuthClient
{
    private static readonly HttpClient HttpClient = new();
    private const string TokenEndpoint = "https://test-identity.monthio.com/connect/token";

    public static async Task<string> GetAccessTokenAsync(string clientCredentialId, string sharedSecret)
    {
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientCredentialId}:{sharedSecret}"));

        var request = new HttpRequestMessage(HttpMethod.Post, TokenEndpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        request.Content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "client_credentials")
        });

        var response = await HttpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("access_token").GetString()
               ?? throw new InvalidOperationException("access_token missing from response");
    }
}
