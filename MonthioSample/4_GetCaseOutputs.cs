using System.Net.Http.Headers;
using System.Text.Json;

namespace MonthioSample;

public class MonthioCaseDataClient
{
    private static readonly HttpClient HttpClient = new();
    private const string BaseUrl = "https://test-budgets.monthio.com/api/v1/case";

    public static async Task<JsonElement> GetCaseAsync(string accessToken, string caseId)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/{caseId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.Accept.Add(new("application/json"));

        var response = await HttpClient.SendAsync(request);
        var json = await response.Content.ReadAsStringAsync();
        response.EnsureSuccessStatusCode();

        return JsonSerializer.Deserialize<JsonElement>(json);
    }

    public static void PrintCaseStatus(JsonElement caseData)
    {
        Console.WriteLine($"  createdOn:       {TryGetDate(caseData, "createdOn")}");
        Console.WriteLine($"  updatedOn:       {TryGetDate(caseData, "updatedOn")}");
        Console.WriteLine($"  finishedOn:      {TryGetDate(caseData, "finishedOn")}");
        Console.WriteLine($"  invitationSentOn:{TryGetDate(caseData, "invitationSentOn")}");
        Console.WriteLine($"  authenticatedOn: {TryGetDate(caseData, "authenticatedOn")}");
        Console.WriteLine($"  outputSentOn:    {TryGetDate(caseData, "outputSentOn")}");

        if (caseData.TryGetProperty("processingStatus", out var ps))
        {
            var userId = ps.TryGetProperty("userId", out var uid) && uid.ValueKind != JsonValueKind.Null
                ? uid.ToString() : "null";
            Console.WriteLine($"  processingStatus: {ps.GetProperty("status").GetString()} (userId: {userId})");
        }

        if (caseData.TryGetProperty("statuses", out var statuses))
        {
            Console.WriteLine("  statuses:");
            foreach (var s in statuses.EnumerateArray())
            {
                var userId = s.TryGetProperty("userId", out var suid) && suid.ValueKind != JsonValueKind.Null
                    ? suid.ToString() : "null";
                Console.WriteLine($"    [{s.GetProperty("status").GetString()}] {s.GetProperty("createdOn").GetDateTime():yyyy-MM-dd HH:mm:ss} userId: {userId}");
            }
        }

        Console.WriteLine();
    }

    private static string TryGetDate(JsonElement el, string prop) =>
        el.TryGetProperty(prop, out var v) && v.ValueKind != JsonValueKind.Null
            ? v.GetDateTime().ToString("yyyy-MM-dd HH:mm:ss")
            : "null";
}
