using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace MonthioSample;

public class MonthioEskatClient
{
    private static readonly HttpClient HttpClient = new();
    private const string DataIngestionEndpoint = "https://test-api.monthio.com/data-ingestion?moduleId=eskat";

    // Maps each ConsumerId to its local eSkat XML file
    private static readonly Dictionary<string, string> EskatFiles = new()
    {
        ["MainApplicant"] = "eskat-sample.xml",
        ["CoApplicant"]   = "eskat-sample_co.xml"
    };

    public static async Task IngestAllApplicantsAsync(CreateCaseResponse caseResponse)
    {
        foreach (var applicant in caseResponse.Applicants)
        {
            if (!EskatFiles.TryGetValue(applicant.ConsumerId, out var xmlFile))
            {
                Console.WriteLine($"  No eSkat file mapped for '{applicant.ConsumerId}', skipping ingestion.");
                continue;
            }

            Console.WriteLine($"Ingesting eSkat data for '{applicant.ConsumerId}' from {xmlFile}...");
            var eskatXml = await File.ReadAllTextAsync(xmlFile);
            await IngestEskatAsync(applicant.DataIngestionToken, eskatXml);
            Console.WriteLine($"  eSkat data ingested.");
        }
    }

    public static void PrintFlowUrls(CreateCaseResponse caseResponse)
    {
        Console.WriteLine("Share these URLs with the applicants to complete the flow:");
        foreach (var applicant in caseResponse.Applicants)
            Console.WriteLine($"  [{applicant.ConsumerId}] https://test-flow.monthio.com/?sessionId={applicant.Id}");
    }

    private static async Task IngestEskatAsync(string dataIngestionToken, string eskatXml)
    {
        // Build JSON manually so $type is serialized as-is (JsonSerializer won't produce $-prefixed keys).
        // JsonSerializer.Serialize on the string handles all required JSON escaping of the XML content.
        var json = $"{{\"data\":{{\"$type\":\"EskatXmlOutput-v1\",\"xml\":{JsonSerializer.Serialize(eskatXml)}}}}}";

        var request = new HttpRequestMessage(HttpMethod.Post, DataIngestionEndpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", dataIngestionToken);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await HttpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }
}
