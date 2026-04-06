using Whycespace.Shared.Contracts.Systems.Intent;

namespace Whycespace.Systems.Midstream.Sandbox;

/// <summary>
/// Sandbox execution workflow — composition only.
/// Orchestrates first economic execution: create sandbox → activate →
/// execute controlled SPV transactions → audit → close.
/// NO execution, NO domain mutation, NO persistence.
///
/// Boundary declaration:
/// - BCs touched: operational-system/deployment/sandbox, structural-system/cluster/spv
/// - Engines composed: none directly — dispatches via runtime
/// - Runtime pipelines: sandbox + core-system validation
/// </summary>
public sealed class SandboxExecutionWorkflow
{
    private readonly ISystemIntentDispatcher _intentDispatcher;

    public SandboxExecutionWorkflow(ISystemIntentDispatcher intentDispatcher)
    {
        _intentDispatcher = intentDispatcher;
    }

    /// <summary>
    /// Creates and activates a sandbox with controlled limits.
    /// </summary>
    public async Task<IntentResult> InitializeSandboxAsync(
        string sandboxName, string regionId,
        decimal capitalCap, int transactionLimit,
        string correlationId, CancellationToken ct = default)
    {
        var createResult = await _intentDispatcher.DispatchAsync(new ExecuteCommandIntent
        {
            CommandId = Guid.Empty,
            CommandType = "operational.deployment.sandbox.create",
            Payload = new { Name = sandboxName, RegionId = regionId, CapitalCap = capitalCap, TransactionLimit = transactionLimit },
            CorrelationId = correlationId,
            Timestamp = default,
            Headers = new Dictionary<string, string> { ["x-target-region"] = regionId, ["x-sandbox"] = "true" }
        }, ct);

        if (!createResult.Success) return createResult;

        return await _intentDispatcher.DispatchAsync(new ExecuteCommandIntent
        {
            CommandId = Guid.Empty,
            CommandType = "operational.deployment.sandbox.activate",
            Payload = new { SandboxId = createResult.Data },
            CorrelationId = correlationId,
            Timestamp = default,
            Headers = new Dictionary<string, string> { ["x-sandbox"] = "true" }
        }, ct);
    }

    /// <summary>
    /// Executes a controlled SPV transaction within the sandbox.
    /// </summary>
    public async Task<IntentResult> ExecuteSandboxTransactionAsync(
        string sandboxId, decimal amount,
        string correlationId, CancellationToken ct = default)
    {
        return await _intentDispatcher.DispatchAsync(new ExecuteCommandIntent
        {
            CommandId = Guid.Empty,
            CommandType = "operational.deployment.sandbox.record-transaction",
            Payload = new { SandboxId = sandboxId, Amount = amount },
            CorrelationId = correlationId,
            Timestamp = default,
            Headers = new Dictionary<string, string> { ["x-sandbox"] = "true" }
        }, ct);
    }

    /// <summary>
    /// Closes the sandbox and produces final audit.
    /// </summary>
    public async Task<IntentResult> CloseSandboxAsync(
        string sandboxId, string correlationId, CancellationToken ct = default)
    {
        return await _intentDispatcher.DispatchAsync(new ExecuteCommandIntent
        {
            CommandId = Guid.Empty,
            CommandType = "operational.deployment.sandbox.close",
            Payload = new { SandboxId = sandboxId },
            CorrelationId = correlationId,
            Timestamp = default,
            Headers = new Dictionary<string, string> { ["x-sandbox"] = "true" }
        }, ct);
    }
}
