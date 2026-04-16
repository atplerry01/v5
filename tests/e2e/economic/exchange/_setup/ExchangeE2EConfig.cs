namespace Whycespace.Tests.E2E.Economic.Exchange.Setup;

/// <summary>
/// E2E configuration for the exchange test suite (fx + rate). Sourced from
/// environment variables so the suite is infrastructure-independent at compile
/// time and CI-injectable at execution time. Required values throw on access
/// when missing — fail fast per E12 spec. Mirrors CapitalE2EConfig.
/// </summary>
public static class ExchangeE2EConfig
{
    public const string EnvApiBaseUrl                  = "EXCHANGE_E2E_API_BASE_URL";
    public const string EnvProjectionsConnectionString = "EXCHANGE_E2E_PROJECTIONS_CONN";
    public const string EnvAuthToken                   = "EXCHANGE_E2E_AUTH_TOKEN";
    public const string EnvRunId                       = "EXCHANGE_E2E_RUN_ID";
    public const string EnvHealthPath                  = "EXCHANGE_E2E_HEALTH_PATH";
    public const string EnvPollTimeoutMs               = "EXCHANGE_E2E_POLL_TIMEOUT_MS";

    public static string ApiBaseUrl =>
        Environment.GetEnvironmentVariable(EnvApiBaseUrl)
        ?? throw new InvalidOperationException(
            $"E2E config missing: set {EnvApiBaseUrl} (e.g. http://localhost:5000).");

    public static string ProjectionsConnectionString =>
        Environment.GetEnvironmentVariable(EnvProjectionsConnectionString)
        ?? throw new InvalidOperationException(
            $"E2E config missing: set {EnvProjectionsConnectionString}.");

    public static string? AuthToken =>
        Environment.GetEnvironmentVariable(EnvAuthToken);

    public static string RunId =>
        Environment.GetEnvironmentVariable(EnvRunId)
        ?? throw new InvalidOperationException(
            $"E2E config missing: set {EnvRunId} (stable per-run nonce; CI sets build id).");

    public static string HealthPath =>
        Environment.GetEnvironmentVariable(EnvHealthPath) ?? "/health";

    public static TimeSpan PollTimeout =>
        int.TryParse(Environment.GetEnvironmentVariable(EnvPollTimeoutMs), out var ms) && ms > 0
            ? TimeSpan.FromMilliseconds(ms)
            : TimeSpan.FromSeconds(15);
}
