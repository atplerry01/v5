namespace Whycespace.Tests.E2E.Economic.Revenue.Setup;

/// <summary>
/// Phase 3 economic pipeline E2E configuration. Mirrors the capital E2E env-var
/// contract so CI can drive both suites with a single config block. All required
/// values throw on access when unset (fail-fast).
/// </summary>
public static class RevenuePipelineE2EConfig
{
    public const string EnvApiBaseUrl                  = "REVENUE_E2E_API_BASE_URL";
    public const string EnvProjectionsConnectionString = "REVENUE_E2E_PROJECTIONS_CONN";
    public const string EnvAuthToken                   = "REVENUE_E2E_AUTH_TOKEN";
    public const string EnvRunId                       = "REVENUE_E2E_RUN_ID";
    public const string EnvHealthPath                  = "REVENUE_E2E_HEALTH_PATH";
    public const string EnvPollTimeoutMs               = "REVENUE_E2E_POLL_TIMEOUT_MS";

    public static string ApiBaseUrl =>
        Environment.GetEnvironmentVariable(EnvApiBaseUrl)
        ?? throw new InvalidOperationException(
            $"Phase 3 E2E config missing: set {EnvApiBaseUrl} (e.g. http://localhost:5000).");

    public static string ProjectionsConnectionString =>
        Environment.GetEnvironmentVariable(EnvProjectionsConnectionString)
        ?? throw new InvalidOperationException(
            $"Phase 3 E2E config missing: set {EnvProjectionsConnectionString}.");

    public static string? AuthToken =>
        Environment.GetEnvironmentVariable(EnvAuthToken);

    public static string RunId =>
        Environment.GetEnvironmentVariable(EnvRunId)
        ?? throw new InvalidOperationException(
            $"Phase 3 E2E config missing: set {EnvRunId} (per-run nonce; CI uses build id).");

    public static string HealthPath =>
        Environment.GetEnvironmentVariable(EnvHealthPath) ?? "/health";

    public static TimeSpan PollTimeout =>
        int.TryParse(Environment.GetEnvironmentVariable(EnvPollTimeoutMs), out var ms) && ms > 0
            ? TimeSpan.FromMilliseconds(ms)
            : TimeSpan.FromSeconds(30);
}
