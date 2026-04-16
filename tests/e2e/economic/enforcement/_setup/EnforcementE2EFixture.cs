using System.Net.Http.Headers;
using Whycespace.Shared.Kernel.Domain;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.E2E.Economic.Enforcement.Setup;

internal sealed class EnforcementE2ERuntimeClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}

public sealed class EnforcementE2EFixture : IAsyncLifetime
{
    public HttpClient Http { get; private set; } = default!;
    public TestIdGenerator IdGenerator { get; } = new();
    public IClock Clock { get; } = new EnforcementE2ERuntimeClock();

    public async Task InitializeAsync()
    {
        Http = new HttpClient { BaseAddress = new Uri(EnforcementE2EConfig.ApiBaseUrl) };

        var token = EnforcementE2EConfig.AuthToken;
        if (!string.IsNullOrWhiteSpace(token))
            Http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        try
        {
            using var probe = await Http.GetAsync(EnforcementE2EConfig.HealthPath);
            if (!probe.IsSuccessStatusCode && probe.StatusCode != System.Net.HttpStatusCode.Unauthorized)
                throw new InvalidOperationException(
                    $"API health probe at {EnforcementE2EConfig.ApiBaseUrl}{EnforcementE2EConfig.HealthPath} returned {(int)probe.StatusCode}. " +
                    "Bring up the stack or correct ENFORCEMENT_E2E_API_BASE_URL before running E2E tests.");
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException(
                $"API unreachable at {EnforcementE2EConfig.ApiBaseUrl}. Start the API host or set ENFORCEMENT_E2E_API_BASE_URL. ({ex.Message})", ex);
        }
    }

    public Task DisposeAsync()
    {
        Http.Dispose();
        return Task.CompletedTask;
    }

    public Guid SeedId(string seed) => IdGenerator.Generate($"e2e:enforcement:{EnforcementE2EConfig.RunId}:{seed}");

    /// <summary>
    /// RunId-scoped unique rule code. The enforcement rule controller seeds
    /// the aggregate id from the rule code, so every run needs a distinct
    /// rule code to avoid collision with prior runs.
    /// </summary>
    public string RunRuleCode(string seed)
    {
        var h = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes($"e2e:enforcement:{EnforcementE2EConfig.RunId}:{seed}"));
        return $"RULE-{Convert.ToHexString(h.AsSpan(0, 4))}";
    }
}
