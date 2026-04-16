using System.Net.Http.Headers;
using Whycespace.Shared.Kernel.Domain;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.E2E.Economic.Exchange.Setup;

// E12/CLOCK-ALIGNMENT: wall-clock-backed IClock for the exchange E2E suite.
// Matches CapitalE2ERuntimeClock rationale — timestamps produced by the host's
// SystemClock must be comparable to values the tests observe after dispatch.
// Aggregate-id determinism is preserved via TestIdGenerator (SHA-256(RunId:seed))
// which does not read the clock.
internal sealed class ExchangeE2ERuntimeClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}

/// <summary>
/// Shared fixture for the exchange E2E suite (fx + rate). Probes API
/// reachability on init (fail-fast per E12 spec), provides HttpClient,
/// deterministic id generator, and wall-clock IClock to test classes.
/// </summary>
public sealed class ExchangeE2EFixture : IAsyncLifetime
{
    public HttpClient Http { get; private set; } = default!;
    public TestIdGenerator IdGenerator { get; } = new();
    public IClock Clock { get; } = new ExchangeE2ERuntimeClock();

    public async Task InitializeAsync()
    {
        Http = new HttpClient { BaseAddress = new Uri(ExchangeE2EConfig.ApiBaseUrl) };

        var token = ExchangeE2EConfig.AuthToken;
        if (!string.IsNullOrWhiteSpace(token))
            Http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        try
        {
            using var probe = await Http.GetAsync(ExchangeE2EConfig.HealthPath);
            if (!probe.IsSuccessStatusCode && probe.StatusCode != System.Net.HttpStatusCode.Unauthorized)
                throw new InvalidOperationException(
                    $"API health probe at {ExchangeE2EConfig.ApiBaseUrl}{ExchangeE2EConfig.HealthPath} returned {(int)probe.StatusCode}. " +
                    "Bring up the stack or correct EXCHANGE_E2E_API_BASE_URL before running E2E tests.");
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException(
                $"API unreachable at {ExchangeE2EConfig.ApiBaseUrl}. Start the API host or set EXCHANGE_E2E_API_BASE_URL. ({ex.Message})", ex);
        }
    }

    public Task DisposeAsync()
    {
        Http.Dispose();
        return Task.CompletedTask;
    }

    public Guid SeedId(string seed) => IdGenerator.Generate($"e2e:exchange:{ExchangeE2EConfig.RunId}:{seed}");

    /// <summary>
    /// RunId-scoped synthetic 3-char currency code used by fx/rate lifecycle
    /// tests to guarantee per-run unique aggregate ids without flipping the
    /// canonical controller-side deterministic seed. The domain accepts any
    /// non-empty currency string; two distinct calls (e.g. <c>Base</c> and
    /// <c>Quote</c>) with the same seed return different codes.
    /// </summary>
    public string RunCurrencyCode(string seed)
    {
        var h = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes($"e2e:exchange:{ExchangeE2EConfig.RunId}:{seed}"));
        // Map the first two bytes to two uppercase letters (A..Z, 26^2 = 676
        // unique combos per RunId — more than enough for the test surface).
        char a = (char)('A' + (h[0] % 26));
        char b = (char)('A' + (h[1] % 26));
        return $"X{a}{b}";
    }
}
