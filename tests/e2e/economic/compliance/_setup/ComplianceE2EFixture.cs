using System.Net.Http.Headers;
using Whycespace.Shared.Kernel.Domain;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.E2E.Economic.Compliance.Setup;

// E12/CLOCK-ALIGNMENT: wall-clock-backed IClock for the compliance E2E suite.
// Matches CapitalE2ERuntimeClock / ExchangeE2ERuntimeClock rationale — values
// produced by the host's SystemClock (RecordedAt, FinalizedAt) must be
// comparable to values the tests observe after dispatch. Aggregate-id
// determinism is preserved via TestIdGenerator (SHA-256 of RunId:seed) which
// does not read the clock.
internal sealed class ComplianceE2ERuntimeClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}

/// <summary>
/// Shared fixture for the compliance E2E suite (audit). Probes API reachability
/// on init (fail-fast per E12 spec), provides HttpClient, deterministic id
/// generator, and wall-clock IClock to test classes.
///
/// Determinism: every aggregate id derived from <c>ComplianceE2EConfig.RunId</c>
/// plus a per-test seed, hashed via <c>TestIdGenerator</c>. Same RunId → same ids.
/// </summary>
public sealed class ComplianceE2EFixture : IAsyncLifetime
{
    public HttpClient Http { get; private set; } = default!;
    public TestIdGenerator IdGenerator { get; } = new();
    public IClock Clock { get; } = new ComplianceE2ERuntimeClock();

    public async Task InitializeAsync()
    {
        Http = new HttpClient { BaseAddress = new Uri(ComplianceE2EConfig.ApiBaseUrl) };

        var token = ComplianceE2EConfig.AuthToken;
        if (!string.IsNullOrWhiteSpace(token))
            Http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        try
        {
            using var probe = await Http.GetAsync(ComplianceE2EConfig.HealthPath);
            if (!probe.IsSuccessStatusCode && probe.StatusCode != System.Net.HttpStatusCode.Unauthorized)
                throw new InvalidOperationException(
                    $"API health probe at {ComplianceE2EConfig.ApiBaseUrl}{ComplianceE2EConfig.HealthPath} returned {(int)probe.StatusCode}. " +
                    "Bring up the stack or correct COMPLIANCE_E2E_API_BASE_URL before running E2E tests.");
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException(
                $"API unreachable at {ComplianceE2EConfig.ApiBaseUrl}. Start the API host or set COMPLIANCE_E2E_API_BASE_URL. ({ex.Message})", ex);
        }
    }

    public Task DisposeAsync()
    {
        Http.Dispose();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Deterministic id derived from RunId + per-test seed. Pure SHA-256 → Guid
    /// (no Guid.NewGuid, no system clock). Identical RunId yields identical ids.
    /// </summary>
    public Guid SeedId(string seed) => IdGenerator.Generate($"e2e:compliance:{ComplianceE2EConfig.RunId}:{seed}");
}
