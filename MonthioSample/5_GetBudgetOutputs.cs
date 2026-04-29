using System.Net.Http.Headers;
using System.Text.Json;

namespace MonthioSample;

public class MonthioBudgetOutputsClient
{
    private static readonly HttpClient HttpClient = new();

    public static async Task<JsonElement> GetBudgetOutputsAsync(string accessToken, string caseId)
    {
        var url = $"https://test-budgets.monthio.com/api/smart-check-sessions/{caseId}/budget-outputs";

        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.Accept.Add(new("application/json"));

        var response = await HttpClient.SendAsync(request);
        var json = await response.Content.ReadAsStringAsync();
        response.EnsureSuccessStatusCode();

        return JsonSerializer.Deserialize<JsonElement>(json);
    }

    public static void PrintBudgetOutputs(JsonElement budgetOutputs)
    {
        if (budgetOutputs.GetArrayLength() == 0)
        {
            Console.WriteLine("- no outputs available");
            return;
        }

        foreach (var item in budgetOutputs.EnumerateArray())
        {
            var name      = item.GetProperty("name").GetString();
            var createdOn = item.GetProperty("createdOn").GetDateTime();

            Console.WriteLine($"--- {name} ({createdOn:yyyy-MM-dd HH:mm:ss}) ---");

            if (item.TryGetProperty("output", out var output) &&
                output.TryGetProperty("insights", out var insights) &&
                insights.TryGetProperty("creditWorthiness", out var cw))
            {
                Console.WriteLine($"  disposableAmount:         {cw.GetProperty("disposableAmount").GetDecimal()}");
                Console.WriteLine($"  requiredDisposableAmount: {cw.GetProperty("requiredDisposableAmount").GetDecimal()}");
                Console.WriteLine($"  excessDisposableAmount:   {cw.GetProperty("excessDisposableAmount").GetDecimal()}");
            }
            else
            {
                Console.WriteLine("  (no creditWorthiness insights available)");
            }

            Console.WriteLine();
        }
    }
}
