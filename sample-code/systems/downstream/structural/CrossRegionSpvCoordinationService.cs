using Whycespace.Shared.Contracts.Systems.Intent;

namespace Whycespace.Systems.Downstream.Structural;

/// <summary>
/// Cross-region SPV coordination — composition only.
/// Dispatches SPV operations across regions via federated routing.
/// NO execution, NO domain mutation, NO persistence.
///
/// Boundary declaration:
/// - BCs touched: structural-system/cluster/spv
/// - Engines composed: none directly — dispatches via runtime
/// - Runtime pipelines: federation routing + policy middleware
/// </summary>
public sealed class CrossRegionSpvCoordinationService
{
    private readonly ISystemIntentDispatcher _intentDispatcher;

    public CrossRegionSpvCoordinationService(ISystemIntentDispatcher intentDispatcher)
    {
        _intentDispatcher = intentDispatcher;
    }

    /// <summary>
    /// Coordinates SPV activation across multiple regions.
    /// Each region receives a federated command via runtime.
    /// </summary>
    public async Task<IntentResult> CoordinateCrossRegionActivationAsync(
        IReadOnlyList<CrossRegionSpvIntent> spvIntents,
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        var results = new List<object>();

        foreach (var spvIntent in spvIntents)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var result = await _intentDispatcher.DispatchAsync(new ExecuteCommandIntent
            {
                CommandId = Guid.Empty,
                CommandType = "structural.cluster.spv.activate",
                Payload = new { Id = spvIntent.SpvId },
                CorrelationId = correlationId,
                Timestamp = default,
                Headers = new Dictionary<string, string>
                {
                    ["x-target-region"] = spvIntent.TargetRegion,
                    ["x-jurisdiction"] = spvIntent.JurisdictionCode,
                    ["x-federated"] = "true"
                }
            }, cancellationToken);

            results.Add(new { spvIntent.SpvId, spvIntent.TargetRegion, result.Success, result.ErrorMessage });
        }

        return IntentResult.Ok(Guid.Empty, results);
    }

    /// <summary>
    /// Suspends SPVs across regions — triggered by governance escalation.
    /// </summary>
    public async Task<IntentResult> CoordinateCrossRegionSuspensionAsync(
        IReadOnlyList<CrossRegionSpvIntent> spvIntents,
        string reason,
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        foreach (var spvIntent in spvIntents)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await _intentDispatcher.DispatchAsync(new ExecuteCommandIntent
            {
                CommandId = Guid.Empty,
                CommandType = "structural.cluster.spv.suspend",
                Payload = new { Id = spvIntent.SpvId, Reason = reason },
                CorrelationId = correlationId,
                Timestamp = default,
                Headers = new Dictionary<string, string>
                {
                    ["x-target-region"] = spvIntent.TargetRegion,
                    ["x-jurisdiction"] = spvIntent.JurisdictionCode,
                    ["x-federated"] = "true"
                }
            }, cancellationToken);
        }

        return IntentResult.Ok(Guid.Empty, $"Suspended {spvIntents.Count} SPVs across regions.");
    }
}

public sealed record CrossRegionSpvIntent
{
    public required string SpvId { get; init; }
    public required string TargetRegion { get; init; }
    public required string JurisdictionCode { get; init; }
}
