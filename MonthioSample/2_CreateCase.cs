using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MonthioSample;

public class CreateCaseRequest
{
    public required string ConfigurationId { get; init; }
    public required List<Applicant> Applicants { get; init; }
}

public class Applicant
{
    public required string ConsumerId { get; init; }
}

public class CreateCaseResponse
{
    public required string Id { get; init; }
    public List<ApplicantResult> Applicants { get; init; } = [];
}

public class ApplicantResult
{
    public required string Id { get; init; }
    public required string ConsumerId { get; init; }
    public required string DataIngestionToken { get; init; }
}

public class MonthioCaseClient
{
    private static readonly HttpClient HttpClient = new();
    private const string CaseEndpoint = "https://test-api.monthio.com/case";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static async Task<CreateCaseResponse> CreateCaseAsync(string accessToken, string configurationId, bool includeCoApplicant)
    {
        var applicants = new List<Applicant> { new() { ConsumerId = "MainApplicant" } };
        if (includeCoApplicant)
            applicants.Add(new Applicant { ConsumerId = "CoApplicant" });

        var caseRequest = new CreateCaseRequest { ConfigurationId = configurationId, Applicants = applicants };

        var request = new HttpRequestMessage(HttpMethod.Post, CaseEndpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = new StringContent(
            JsonSerializer.Serialize(caseRequest, JsonOptions),
            Encoding.UTF8,
            "application/json"
        );

        var response = await HttpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<CreateCaseResponse>(json, JsonOptions)
               ?? throw new InvalidOperationException("Failed to deserialize create case response");
    }
}
