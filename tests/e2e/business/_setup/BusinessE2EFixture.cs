using System.Net.Http.Headers;
using Whycespace.Shared.Kernel.Domain;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.E2E.Business.Setup;

// E12.X/CLOCK-ALIGNMENT: wall-clock-backed IClock for the business E2E suite.
//
// The business E2E suite talks to an out-of-process API host which runs the
// canonical Whyce `SystemClock` (DateTimeOffset.UtcNow). Values that must be
// meaningful to that runtime (e.g. ContractCreatedEvent.CreatedAt, whose
// downstream projection LastUpdatedAt semantics need a real wall-clock
// ordering) cannot be derived from a frozen TestClock without producing
// time-paradoxes across requests.
//
// The fixture therefore exposes a wall-clock-backed IClock under the same
// `Clock` property name so the existing `_fix.Clock.UtcNow + timespan`
// pattern stays consistent with the capital suite. Determinism of aggregate
// ids is preserved separately by `TestIdGenerator` (SHA-256 of `RunId:seed`),
// which does not read the clock.
internal sealed class BusinessE2ERuntimeClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}

/// <summary>
/// Shared fixture for the business-system E2E suite (bootstrap module:
/// <c>BusinessSystemCompositionRoot</c>). Probes API reachability on init
/// (fail-fast per E12 spec), provides a configured HttpClient, deterministic
/// id generator, and wall-clock IClock to every business test class.
///
/// Topic roots covered: <c>whyce.business.agreement.*</c>.
/// Projection prefix: <c>projection_business_agreement_*</c>.
///
/// Determinism: every aggregate id is derived from BusinessE2EConfig.RunId
/// plus a per-test seed, hashed via TestIdGenerator. Same RunId → same ids.
/// </summary>
public sealed class BusinessE2EFixture : IAsyncLifetime
{
    public HttpClient Http { get; private set; } = default!;
    public TestIdGenerator IdGenerator { get; } = new();
    public IClock Clock { get; } = new BusinessE2ERuntimeClock();

    public async Task InitializeAsync()
    {
        Http = new HttpClient { BaseAddress = new Uri(BusinessE2EConfig.ApiBaseUrl) };

        var token = BusinessE2EConfig.AuthToken;
        if (!string.IsNullOrWhiteSpace(token))
            Http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        try
        {
            using var probe = await Http.GetAsync(BusinessE2EConfig.HealthPath);
            if (!probe.IsSuccessStatusCode && probe.StatusCode != System.Net.HttpStatusCode.Unauthorized)
                throw new InvalidOperationException(
                    $"API health probe at {BusinessE2EConfig.ApiBaseUrl}{BusinessE2EConfig.HealthPath} returned {(int)probe.StatusCode}. " +
                    "Bring up the stack or correct BUSINESS_E2E_API_BASE_URL before running E2E tests.");
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException(
                $"API unreachable at {BusinessE2EConfig.ApiBaseUrl}. Start the API host or set BUSINESS_E2E_API_BASE_URL. ({ex.Message})", ex);
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
    public Guid SeedId(string seed) => IdGenerator.Generate($"e2e:business:{BusinessE2EConfig.RunId}:{seed}");
}
