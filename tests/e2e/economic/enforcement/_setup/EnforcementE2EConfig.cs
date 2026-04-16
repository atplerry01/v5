namespace Whycespace.Tests.E2E.Economic.Enforcement.Setup;

public static class EnforcementE2EConfig
{
    public const string EnvApiBaseUrl                  = "ENFORCEMENT_E2E_API_BASE_URL";
    public const string EnvProjectionsConnectionString = "ENFORCEMENT_E2E_PROJECTIONS_CONN";
    public const string EnvAuthToken                   = "ENFORCEMENT_E2E_AUTH_TOKEN";
    public const string EnvRunId                       = "ENFORCEMENT_E2E_RUN_ID";
    public const string EnvHealthPath                  = "ENFORCEMENT_E2E_HEALTH_PATH";
    public const string EnvPollTimeoutMs               = "ENFORCEMENT_E2E_POLL_TIMEOUT_MS";

    public static string ApiBaseUrl =>
        Environment.GetEnvironmentVariable(EnvApiBaseUrl)
        ?? throw new InvalidOperationException($"E2E config missing: set {EnvApiBaseUrl}.");

    public static string ProjectionsConnectionString =>
        Environment.GetEnvironmentVariable(EnvProjectionsConnectionString)
        ?? throw new InvalidOperationException($"E2E config missing: set {EnvProjectionsConnectionString}.");

    public static string? AuthToken => Environment.GetEnvironmentVariable(EnvAuthToken);

    public static string RunId =>
        Environment.GetEnvironmentVariable(EnvRunId)
        ?? throw new InvalidOperationException($"E2E config missing: set {EnvRunId}.");

    public static string HealthPath =>
        Environment.GetEnvironmentVariable(EnvHealthPath) ?? "/health";

    public static TimeSpan PollTimeout =>
        int.TryParse(Environment.GetEnvironmentVariable(EnvPollTimeoutMs), out var ms) && ms > 0
            ? TimeSpan.FromMilliseconds(ms)
            : TimeSpan.FromSeconds(15);
}
