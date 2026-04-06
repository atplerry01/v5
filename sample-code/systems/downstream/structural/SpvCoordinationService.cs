using Whycespace.Shared.Contracts.Systems;
using Whycespace.Shared.Contracts.Systems.Intent;

namespace Whycespace.Systems.Downstream.Structural;

/// <summary>
/// SPV coordination service — enables bulk operations across SPVs within a subcluster.
/// Systems layer — pure orchestration via intent dispatch.
/// </summary>
public sealed class SpvCoordinationService
{
    private readonly ISystemIntentDispatcher _intentDispatcher;

    public SpvCoordinationService(ISystemIntentDispatcher intentDispatcher)
    {
        _intentDispatcher = intentDispatcher ?? throw new ArgumentNullException(nameof(intentDispatcher));
    }

    public async Task<IntentResult> BulkActivateSpvsAsync(
        IReadOnlyList<string> spvIds,
        string clusterId,
        CancellationToken cancellationToken = default)
    {
        var bulkCommandId = Guid.NewGuid();
        var correlationId = Guid.NewGuid().ToString();
        var results = new List<object>();

        foreach (var spvId in spvIds)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var result = await _intentDispatcher.DispatchAsync(new ExecuteCommandIntent
            {
                CommandId = Guid.NewGuid(),
                CommandType = "structural.cluster.spv.activate",
                Payload = new { Id = spvId },
                CorrelationId = correlationId,
                Timestamp = DateTimeOffset.UtcNow
            }, cancellationToken);

            results.Add(new { SpvId = spvId, Success = result.Success, Error = result.ErrorMessage });
        }

        var allSucceeded = results.All(r => ((dynamic)r).Success);
        return allSucceeded
            ? IntentResult.Ok(bulkCommandId, results)
            : IntentResult.Fail(bulkCommandId, $"Some SPV activations failed in cluster '{clusterId}'");
    }

    public async Task<IntentResult> SuspendAllSpvsAsync(
        string subClusterId,
        string reason,
        IReadOnlyList<string> spvIds,
        CancellationToken cancellationToken = default)
    {
        var bulkCommandId = Guid.NewGuid();
        var correlationId = Guid.NewGuid().ToString();

        foreach (var spvId in spvIds)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await _intentDispatcher.DispatchAsync(new ExecuteCommandIntent
            {
                CommandId = Guid.NewGuid(),
                CommandType = "structural.cluster.spv.suspend",
                Payload = new { Id = spvId, Reason = reason },
                CorrelationId = correlationId,
                Timestamp = DateTimeOffset.UtcNow
            }, cancellationToken);
        }

        return IntentResult.Ok(bulkCommandId, $"Suspended {spvIds.Count} SPVs in subcluster '{subClusterId}'");
    }
}
