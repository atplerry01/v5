using System.Net.Http.Headers;
using Whycespace.Shared.Kernel.Domain;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.E2E.Economic.Capital.Setup;

// E12.X/CLOCK-ALIGNMENT: wall-clock-backed IClock for the capital E2E suite.
//
// The capital E2E suite talks to an out-of-process API host which runs the
// canonical Whyce `SystemClock` (DateTimeOffset.UtcNow). Values that must be
// meaningful to that runtime (e.g. Reserve.expiresAt, whose aggregate
// invariant `expiresAt > reservedAt` is evaluated against the host's
// reservedAt = SystemClock.UtcNow) cannot be derived from a frozen TestClock
// without producing a guaranteed invariant violation on every run.
//
// The fixture therefore exposes a wall-clock-backed IClock under the same
// `Clock` property name so the existing `_fix.Clock.UtcNow + timespan`
// pattern in tests keeps working without each test reaching for
// DateTimeOffset.UtcNow directly. Determinism of aggregate ids is preserved
// separately by `TestIdGenerator` (SHA-256 of `RunId:seed`), which does not
// read the clock.
internal sealed class CapitalE2ERuntimeClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}

/// <summary>
/// Shared fixture for the capital E2E suite. Probes API reachability on init
/// (fail-fast per E12 spec), provides a configured HttpClient, deterministic
/// id generator, and frozen test clock to every domain test class.
///
/// Determinism: every aggregate id is derived from CapitalE2EConfig.RunId
/// plus a per-test seed, hashed via TestIdGenerator. Same RunId → same ids.
/// </summary>
public sealed class CapitalE2EFixture : IAsyncLifetime
{
    public HttpClient Http { get; private set; } = default!;
    public TestIdGenerator IdGenerator { get; } = new();
    public IClock Clock { get; } = new CapitalE2ERuntimeClock();

    public async Task InitializeAsync()
    {
        Http = new HttpClient { BaseAddress = new Uri(CapitalE2EConfig.ApiBaseUrl) };

        var token = CapitalE2EConfig.AuthToken;
        if (!string.IsNullOrWhiteSpace(token))
            Http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        try
        {
            using var probe = await Http.GetAsync(CapitalE2EConfig.HealthPath);
            if (!probe.IsSuccessStatusCode && probe.StatusCode != System.Net.HttpStatusCode.Unauthorized)
                throw new InvalidOperationException(
                    $"API health probe at {CapitalE2EConfig.ApiBaseUrl}{CapitalE2EConfig.HealthPath} returned {(int)probe.StatusCode}. " +
                    "Bring up the stack or correct CAPITAL_E2E_API_BASE_URL before running E2E tests.");
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException(
                $"API unreachable at {CapitalE2EConfig.ApiBaseUrl}. Start the API host or set CAPITAL_E2E_API_BASE_URL. ({ex.Message})", ex);
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
    public Guid SeedId(string seed) => IdGenerator.Generate($"e2e:capital:{CapitalE2EConfig.RunId}:{seed}");
}
