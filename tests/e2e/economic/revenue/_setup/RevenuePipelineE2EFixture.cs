using System.Net.Http.Headers;
using Whycespace.Shared.Kernel.Domain;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.E2E.Economic.Revenue.Setup;

internal sealed class RevenuePipelineE2ERuntimeClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}

/// <summary>
/// Phase 3 (T3.7) shared fixture. Probes API health on init (fail-fast),
/// hands every test a configured HttpClient, deterministic id generator, and
/// a wall-clock IClock aligned with the host runtime.
/// </summary>
public sealed class RevenuePipelineE2EFixture : IAsyncLifetime
{
    public HttpClient Http { get; private set; } = default!;
    public TestIdGenerator IdGenerator { get; } = new();
    public IClock Clock { get; } = new RevenuePipelineE2ERuntimeClock();

    public async Task InitializeAsync()
    {
        Http = new HttpClient { BaseAddress = new Uri(RevenuePipelineE2EConfig.ApiBaseUrl) };

        var token = RevenuePipelineE2EConfig.AuthToken;
        if (!string.IsNullOrWhiteSpace(token))
            Http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        try
        {
            using var probe = await Http.GetAsync(RevenuePipelineE2EConfig.HealthPath);
            if (!probe.IsSuccessStatusCode && probe.StatusCode != System.Net.HttpStatusCode.Unauthorized)
                throw new InvalidOperationException(
                    $"API health probe at {RevenuePipelineE2EConfig.ApiBaseUrl}{RevenuePipelineE2EConfig.HealthPath} returned {(int)probe.StatusCode}. " +
                    "Bring up the stack or correct REVENUE_E2E_API_BASE_URL before running Phase 3 E2E tests.");
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException(
                $"API unreachable at {RevenuePipelineE2EConfig.ApiBaseUrl}. Start the API host or set REVENUE_E2E_API_BASE_URL. ({ex.Message})", ex);
        }
    }

    public Task DisposeAsync()
    {
        Http.Dispose();
        return Task.CompletedTask;
    }

    public Guid SeedId(string seed) => IdGenerator.Generate($"e2e:revenue-pipeline:{RevenuePipelineE2EConfig.RunId}:{seed}");
}
