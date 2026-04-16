namespace Whycespace.Shared.Contracts.Economic.Reconciliation.Workflow;

/// <summary>
/// Read-side lookup for the reconciliation lifecycle workflow. Queries the
/// workflow projection to resolve the ProcessId associated with a
/// DiscrepancyId so the lifecycle handler can close the loop on
/// DiscrepancyResolvedEvent by issuing ResolveReconciliationCommand.
///
/// This is a read-only projection query — no domain mutation, no aggregate
/// access.
/// </summary>
public interface IReconciliationWorkflowLookup
{
    Task<Guid?> FindProcessIdByDiscrepancyAsync(Guid discrepancyId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Write-side store for the reconciliation workflow projection. Materialised
/// by <c>ReconciliationWorkflowProjectionHandler</c>. Consumed by the
/// lifecycle handler through <see cref="IReconciliationWorkflowLookup"/>.
/// </summary>
public interface IReconciliationWorkflowStore
{
    Task<ReconciliationWorkflowReadModel?> GetByProcessAsync(Guid processId, CancellationToken cancellationToken = default);
    Task<ReconciliationWorkflowReadModel?> GetByDiscrepancyAsync(Guid discrepancyId, CancellationToken cancellationToken = default);

    Task UpsertByProcessAsync(ReconciliationWorkflowReadModel model, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the state of the workflow row keyed by <paramref name="discrepancyId"/>.
    /// No-op when no row carries that discrepancy id — discrepancy events for
    /// non-workflow contexts are ignored.
    /// </summary>
    Task UpdateStateByDiscrepancyAsync(Guid discrepancyId, string newState, string lastEvent, DateTimeOffset updatedAt, CancellationToken cancellationToken = default);
}
