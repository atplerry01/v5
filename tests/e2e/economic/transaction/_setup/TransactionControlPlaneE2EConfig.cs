namespace Whycespace.Tests.E2E.Economic.Transaction.Setup;

public static class TransactionControlPlaneE2EConfig
{
    public const string EnvApiBaseUrl                  = "TXN_E2E_API_BASE_URL";
    public const string EnvProjectionsConnectionString = "TXN_E2E_PROJECTIONS_CONN";
    public const string EnvAuthToken                   = "TXN_E2E_AUTH_TOKEN";
    public const string EnvRunId                       = "TXN_E2E_RUN_ID";
    public const string EnvHealthPath                  = "TXN_E2E_HEALTH_PATH";
    public const string EnvPollTimeoutMs               = "TXN_E2E_POLL_TIMEOUT_MS";

    public static string ApiBaseUrl =>
        Environment.GetEnvironmentVariable(EnvApiBaseUrl)
        ?? throw new InvalidOperationException($"Phase 4 E2E config missing: set {EnvApiBaseUrl}.");

    public static string ProjectionsConnectionString =>
        Environment.GetEnvironmentVariable(EnvProjectionsConnectionString)
        ?? throw new InvalidOperationException($"Phase 4 E2E config missing: set {EnvProjectionsConnectionString}.");

    public static string? AuthToken => Environment.GetEnvironmentVariable(EnvAuthToken);

    public static string RunId =>
        Environment.GetEnvironmentVariable(EnvRunId)
        ?? throw new InvalidOperationException($"Phase 4 E2E config missing: set {EnvRunId}.");

    public static string HealthPath =>
        Environment.GetEnvironmentVariable(EnvHealthPath) ?? "/health";

    public static TimeSpan PollTimeout =>
        int.TryParse(Environment.GetEnvironmentVariable(EnvPollTimeoutMs), out var ms) && ms > 0
            ? TimeSpan.FromMilliseconds(ms)
            : TimeSpan.FromSeconds(30);
}
