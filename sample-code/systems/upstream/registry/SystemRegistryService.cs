using Whycespace.Shared.Contracts.Systems;
using Whycespace.Shared.Contracts.Systems.Intent;

namespace Whycespace.Systems.Upstream.Registry;

/// <summary>
/// System registry — enumerates all bounded contexts, engine coverage, and system health.
/// Systems layer — pure orchestration, no execution.
/// </summary>
public sealed class SystemRegistryService
{
    private readonly ISystemIntentDispatcher _intentDispatcher;

    private static readonly IReadOnlyList<SystemRegistration> RegisteredSystems =
    [
        new("trust-system", "identity", "WhyceID identity lifecycle", ["authentication", "authorization", "consent", "device", "federation", "session", "trust"]),
        new("constitutional-system", "policy", "WhycePolicy enforcement", ["enforcement", "evaluation", "federation", "registry", "simulation"]),
        new("constitutional-system", "chain", "WhyceChain ledger", ["write", "verification", "anchoring"]),
        new("decision-system", "governance", "Guardian governance", ["proposal", "voting", "quorum", "delegation"]),
        new("economic-system", "capital", "Capital management", ["asset", "binding", "capital", "reserve", "vault"]),
        new("economic-system", "ledger", "Ledger + settlement", ["ledger", "obligation", "settlement", "treasury"]),
        new("economic-system", "revenue", "Revenue + distribution", ["distribution", "payout", "pricing", "revenue"]),
        new("economic-system", "transaction", "Transactions", ["charge", "limit", "transaction", "wallet"]),
        new("economic-system", "enforcement", "Economic enforcement", ["enforcement"]),
        new("structural-system", "cluster", "Cluster orchestration", ["authority", "classification", "cluster", "lifecycle", "spv", "subcluster", "topology"]),
        new("orchestration-system", "workflow", "Workflow execution", ["assignment", "compensation", "definition", "execution", "instance"]),
        new("operational-system", "incident", "Incident response", ["incident"]),
        new("intelligence-system", "observability", "System observability", ["alert", "diagnostic", "health", "metric", "trace"]),
    ];

    public SystemRegistryService(ISystemIntentDispatcher intentDispatcher)
    {
        _intentDispatcher = intentDispatcher;
    }

    public IReadOnlyList<SystemRegistration> GetRegisteredSystems() => RegisteredSystems;

    public async Task<IntentResult> GetSystemHealthAsync(string systemName, CancellationToken cancellationToken = default)
    {
        return await _intentDispatcher.DispatchAsync(new ExecuteCommandIntent
        {
            CommandId = Guid.NewGuid(),
            CommandType = "observability.health.check",
            Payload = new { SystemName = systemName },
            CorrelationId = Guid.NewGuid().ToString(),
            Timestamp = DateTimeOffset.UtcNow
        }, cancellationToken);
    }
}

public sealed record SystemRegistration(
    string BoundedContext,
    string Domain,
    string Description,
    IReadOnlyList<string> Modules);
