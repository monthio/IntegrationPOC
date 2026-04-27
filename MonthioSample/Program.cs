using MonthioSample;

/*
Run it locally with
MONTHIO_CLIENT_ID=xxx MONTHIO_CLIENT_SECRET=xxx MONTHIO_CONFIGURATION_ID=xxx dotnet run

Or in docker
docker build -f MonthioSample/Dockerfile -t monthio-sample . \
    && docker run -it -e MONTHIO_CLIENT_ID=xxx -e MONTHIO_CLIENT_SECRET=xxx -e MONTHIO_CONFIGURATION_ID=xxx monthio-sample

Add MONTHIO_CASE_ID=xxx if you just want to run get case endpoints for existing case
Add MONTHIO_INCLUDE_CO_APPLICANT=true if you want co-applicant to be added

MONTHIO_CLIENT_ID is the clientCredentialId
MONTHIO_CLIENT_SECRET is the shared secret for that clientCredential
    Log into Monthio system with technical admin privilege, go to "Setup" > "Client credentials" > "New client credential"
    See SetupSharedSecret.gif
MONTHIO_CONFIGURATION_ID is configuration id inside Monthio system
    Log into Monthio system with admin privilege, go to "Credit Settings" > "Configurations"
    For eskat to be ingestable, enable eskat module and "Post Eskat data with case creation" setting
    See SetupIngestEskat.gif
 */

var clientCredentialId = Environment.GetEnvironmentVariable("MONTHIO_CLIENT_ID")
    ?? throw new InvalidOperationException("MONTHIO_CLIENT_ID env var not set");
var sharedSecret = Environment.GetEnvironmentVariable("MONTHIO_CLIENT_SECRET")
    ?? throw new InvalidOperationException("MONTHIO_CLIENT_SECRET env var not set");

// 1. Authenticate
Console.WriteLine("Requesting Monthio access token...");
var accessToken = await MonthioAuthClient.GetAccessTokenAsync(clientCredentialId, sharedSecret);
Console.WriteLine("Access token acquired.");

// 2-3. Create case + ingest eSkat (skipped when MONTHIO_CASE_ID is already set)
var caseId = Environment.GetEnvironmentVariable("MONTHIO_CASE_ID");

if (caseId is null)
{
    var configurationId = Environment.GetEnvironmentVariable("MONTHIO_CONFIGURATION_ID")
        ?? throw new InvalidOperationException("MONTHIO_CONFIGURATION_ID env var not set");
    var includeCoApplicant = string.Equals(
        Environment.GetEnvironmentVariable("MONTHIO_INCLUDE_CO_APPLICANT"), "true",
        StringComparison.OrdinalIgnoreCase);

    Console.WriteLine($"Creating case (co-applicant: {includeCoApplicant})...");
    var caseResponse = await MonthioCaseClient.CreateCaseAsync(accessToken, configurationId, includeCoApplicant);
    caseId = caseResponse.Id;
    Console.WriteLine($"Case created. CaseId: {caseId}");

    await MonthioEskatClient.IngestAllApplicantsAsync(caseResponse);
    MonthioEskatClient.PrintFlowUrls(caseResponse);

    Console.WriteLine();
    Console.WriteLine("Press Enter once all applicants have completed the flow...");
    Console.ReadLine();
}

// 4. Fetch and print case status
Console.WriteLine("Fetching case data...");
var caseData = await MonthioCaseDataClient.GetCaseAsync(accessToken, caseId!);
MonthioCaseDataClient.PrintCaseStatus(caseData);

// 5. Fetch and print budget outputs
Console.WriteLine("Fetching budget outputs...");
var budgetOutputs = await MonthioBudgetOutputsClient.GetBudgetOutputsAsync(accessToken, caseData);
MonthioBudgetOutputsClient.PrintBudgetOutputs(budgetOutputs);
