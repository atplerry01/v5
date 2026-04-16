using System.Net.Http.Headers;
using Whycespace.Shared.Kernel.Domain;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.E2E.Economic.Risk.Setup;

internal sealed class RiskE2ERuntimeClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}

/// <summary>
/// Shared fixture for the risk E2E suite (exposure). Mirrors CapitalE2EFixture
/// pattern: probes API reachability on init, provides HttpClient +
/// deterministic id generator + wall-clock IClock.
/// </summary>
public sealed class RiskE2EFixture : IAsyncLifetime
{
    public HttpClient Http { get; private set; } = default!;
    public TestIdGenerator IdGenerator { get; } = new();
    public IClock Clock { get; } = new RiskE2ERuntimeClock();

    public async Task InitializeAsync()
    {
        Http = new HttpClient { BaseAddress = new Uri(RiskE2EConfig.ApiBaseUrl) };

        var token = RiskE2EConfig.AuthToken;
        if (!string.IsNullOrWhiteSpace(token))
            Http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        try
        {
            using var probe = await Http.GetAsync(RiskE2EConfig.HealthPath);
            if (!probe.IsSuccessStatusCode && probe.StatusCode != System.Net.HttpStatusCode.Unauthorized)
                throw new InvalidOperationException(
                    $"API health probe at {RiskE2EConfig.ApiBaseUrl}{RiskE2EConfig.HealthPath} returned {(int)probe.StatusCode}. " +
                    "Bring up the stack or correct RISK_E2E_API_BASE_URL before running E2E tests.");
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException(
                $"API unreachable at {RiskE2EConfig.ApiBaseUrl}. Start the API host or set RISK_E2E_API_BASE_URL. ({ex.Message})", ex);
        }
    }

    public Task DisposeAsync()
    {
        Http.Dispose();
        return Task.CompletedTask;
    }

    public Guid SeedId(string seed) => IdGenerator.Generate($"e2e:risk:{RiskE2EConfig.RunId}:{seed}");
}
