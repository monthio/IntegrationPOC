using System.Net.Http.Headers;
using System.Text.Json;

namespace MonthioSample;

public class MonthioEskatJsonOutputClient
{
    private static readonly HttpClient HttpClient = new();
    private const string BaseUrl = "https://test-api.monthio.com/cases";

    public static async Task GetAllApplicantsEskatJsonAsync(string accessToken, JsonElement caseData)
    {
        var caseId = caseData.GetProperty("id").GetString()
            ?? throw new InvalidOperationException("id missing from case data");

        // Main applicant
        var mainApplicantId = caseData.GetProperty("applicantId").GetString()
            ?? throw new InvalidOperationException("applicantId missing from case data");

        await FetchAndPrintAsync(accessToken, caseId, mainApplicantId, "MainApplicant");

        // Co-applicants (may be empty)
        if (caseData.TryGetProperty("parringApplicants", out var parring))
        {
            foreach (var pa in parring.EnumerateArray())
            {
                var applicantId = pa.GetProperty("applicantId").GetString()
                    ?? throw new InvalidOperationException("parringApplicant applicantId missing");
                await FetchAndPrintAsync(accessToken, caseId, applicantId, "CoApplicant");
            }
        }
    }

    private static async Task FetchAndPrintAsync(string accessToken, string caseId, string applicantId, string label)
    {
        Console.WriteLine($"  [{label}] Fetching eSkat JSON...");

        var url = $"{BaseUrl}/{caseId}/applicants/{applicantId}/eskat/data";
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.Accept.Add(new("application/json"));

        var response = await HttpClient.SendAsync(request);
        var json = await response.Content.ReadAsStringAsync();
        response.EnsureSuccessStatusCode();

        var data = JsonSerializer.Deserialize<JsonElement>(json);

        var cpr = data
            .GetProperty("indkomstOplysningPersonField")
            .GetProperty("personCivilRegistrationIdentifierField")
            .GetString();

        Console.WriteLine($"    cpr:         {cpr}");
        Console.WriteLine($"    json length: {json.Length}");
        Console.WriteLine();
    }
}
