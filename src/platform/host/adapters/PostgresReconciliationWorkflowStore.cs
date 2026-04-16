using Npgsql;
using Whycespace.Shared.Contracts.Economic.Reconciliation.Workflow;

namespace Whycespace.Platform.Host.Adapters;

/// <summary>
/// Postgres-backed store for the reconciliation workflow projection.
/// Implements both the read-side <see cref="IReconciliationWorkflowLookup"/>
/// and the write-side <see cref="IReconciliationWorkflowStore"/> against the
/// schema <c>projection_economic_reconciliation_workflow.workflow_state</c>.
///
/// The table is keyed by <c>process_id</c> with a secondary index on
/// <c>discrepancy_id</c> so lifecycle events that only carry the discrepancy
/// aggregate id can still update the correct workflow row.
/// </summary>
public sealed class PostgresReconciliationWorkflowStore :
    IReconciliationWorkflowLookup,
    IReconciliationWorkflowStore
{
    private readonly ProjectionsDataSource _dataSource;

    public PostgresReconciliationWorkflowStore(ProjectionsDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<Guid?> FindProcessIdByDiscrepancyAsync(Guid discrepancyId, CancellationToken cancellationToken = default)
    {
        await using var conn = await _dataSource.Inner.OpenConnectionAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(
            "SELECT process_id FROM projection_economic_reconciliation_workflow.workflow_state " +
            "WHERE discrepancy_id = @discrepancy_id LIMIT 1", conn);
        cmd.Parameters.AddWithValue("discrepancy_id", discrepancyId);

        var result = await cmd.ExecuteScalarAsync(cancellationToken);
        return result is Guid g ? g : null;
    }

    public async Task<ReconciliationWorkflowReadModel?> GetByProcessAsync(Guid processId, CancellationToken cancellationToken = default)
    {
        await using var conn = await _dataSource.Inner.OpenConnectionAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(
            "SELECT process_id, discrepancy_id, current_state, last_event, correlation_id, updated_at " +
            "FROM projection_economic_reconciliation_workflow.workflow_state " +
            "WHERE process_id = @process_id LIMIT 1", conn);
        cmd.Parameters.AddWithValue("process_id", processId);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) return null;

        return ReadRow(reader);
    }

    public async Task<ReconciliationWorkflowReadModel?> GetByDiscrepancyAsync(Guid discrepancyId, CancellationToken cancellationToken = default)
    {
        await using var conn = await _dataSource.Inner.OpenConnectionAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(
            "SELECT process_id, discrepancy_id, current_state, last_event, correlation_id, updated_at " +
            "FROM projection_economic_reconciliation_workflow.workflow_state " +
            "WHERE discrepancy_id = @discrepancy_id LIMIT 1", conn);
        cmd.Parameters.AddWithValue("discrepancy_id", discrepancyId);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) return null;

        return ReadRow(reader);
    }

    public async Task UpsertByProcessAsync(ReconciliationWorkflowReadModel model, CancellationToken cancellationToken = default)
    {
        await using var conn = await _dataSource.Inner.OpenConnectionAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(
            """
            INSERT INTO projection_economic_reconciliation_workflow.workflow_state
                (process_id, discrepancy_id, current_state, last_event, correlation_id, updated_at)
            VALUES (@process_id, @discrepancy_id, @current_state, @last_event, @correlation_id, @updated_at)
            ON CONFLICT (process_id) DO UPDATE SET
                discrepancy_id = COALESCE(EXCLUDED.discrepancy_id, projection_economic_reconciliation_workflow.workflow_state.discrepancy_id),
                current_state  = EXCLUDED.current_state,
                last_event     = EXCLUDED.last_event,
                correlation_id = COALESCE(EXCLUDED.correlation_id, projection_economic_reconciliation_workflow.workflow_state.correlation_id),
                updated_at     = EXCLUDED.updated_at
            """, conn);
        cmd.Parameters.AddWithValue("process_id", model.ProcessId);
        cmd.Parameters.AddWithValue("discrepancy_id", (object?)model.DiscrepancyId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("current_state", model.CurrentState);
        cmd.Parameters.AddWithValue("last_event", model.LastEvent);
        cmd.Parameters.AddWithValue("correlation_id", model.CorrelationId == Guid.Empty ? DBNull.Value : (object)model.CorrelationId);
        cmd.Parameters.AddWithValue("updated_at", model.UpdatedAt);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task UpdateStateByDiscrepancyAsync(Guid discrepancyId, string newState, string lastEvent, DateTimeOffset updatedAt, CancellationToken cancellationToken = default)
    {
        await using var conn = await _dataSource.Inner.OpenConnectionAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(
            """
            UPDATE projection_economic_reconciliation_workflow.workflow_state
            SET current_state = @current_state,
                last_event    = @last_event,
                updated_at    = @updated_at
            WHERE discrepancy_id = @discrepancy_id
            """, conn);
        cmd.Parameters.AddWithValue("current_state", newState);
        cmd.Parameters.AddWithValue("last_event", lastEvent);
        cmd.Parameters.AddWithValue("updated_at", updatedAt);
        cmd.Parameters.AddWithValue("discrepancy_id", discrepancyId);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private static ReconciliationWorkflowReadModel ReadRow(NpgsqlDataReader reader) => new()
    {
        ProcessId       = reader.GetGuid(0),
        DiscrepancyId   = reader.IsDBNull(1) ? null : reader.GetGuid(1),
        CurrentState    = reader.GetString(2),
        LastEvent       = reader.GetString(3),
        CorrelationId   = reader.IsDBNull(4) ? Guid.Empty : reader.GetGuid(4),
        UpdatedAt       = reader.GetFieldValue<DateTimeOffset>(5)
    };
}
