using System.Net.Http.Headers;
using Whycespace.Shared.Kernel.Domain;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.E2E.Economic.Transaction.Setup;

internal sealed class TransactionControlPlaneE2ERuntimeClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}

public sealed class TransactionControlPlaneE2EFixture : IAsyncLifetime
{
    public HttpClient Http { get; private set; } = default!;
    public TestIdGenerator IdGenerator { get; } = new();
    public IClock Clock { get; } = new TransactionControlPlaneE2ERuntimeClock();

    public async Task InitializeAsync()
    {
        Http = new HttpClient { BaseAddress = new Uri(TransactionControlPlaneE2EConfig.ApiBaseUrl) };

        var token = TransactionControlPlaneE2EConfig.AuthToken;
        if (!string.IsNullOrWhiteSpace(token))
            Http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        try
        {
            using var probe = await Http.GetAsync(TransactionControlPlaneE2EConfig.HealthPath);
            if (!probe.IsSuccessStatusCode && probe.StatusCode != System.Net.HttpStatusCode.Unauthorized)
                throw new InvalidOperationException(
                    $"API health probe at {TransactionControlPlaneE2EConfig.ApiBaseUrl}{TransactionControlPlaneE2EConfig.HealthPath} returned {(int)probe.StatusCode}. " +
                    "Bring up the stack or correct TXN_E2E_API_BASE_URL.");
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException(
                $"API unreachable at {TransactionControlPlaneE2EConfig.ApiBaseUrl}. Start the API host or set TXN_E2E_API_BASE_URL. ({ex.Message})", ex);
        }
    }

    public Task DisposeAsync()
    {
        Http.Dispose();
        return Task.CompletedTask;
    }

    public Guid SeedId(string seed) =>
        IdGenerator.Generate($"e2e:transaction-control-plane:{TransactionControlPlaneE2EConfig.RunId}:{seed}");
}

[CollectionDefinition(Name)]
public sealed class TransactionControlPlaneE2ECollection : ICollectionFixture<TransactionControlPlaneE2EFixture>
{
    public const string Name = "TransactionControlPlaneE2E";
}
