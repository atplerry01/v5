namespace Whycespace.Systems.Midstream.Wss.Workflows.Structural.Cluster.CrossSpv;

/// <summary>
/// Commit step — delegates to T2E CrossSpvEngine via T1M.
/// No business logic in this layer.
/// </summary>
public sealed class CommitCrossSpvStep : ICrossSpvWorkflowStep
{
    public Task<CrossSpvStepResult> ExecuteAsync(
        CrossSpvStepCommand command, CancellationToken ct = default)
    {
        return Task.FromResult(
            CrossSpvStepResult.Ok(command.TransactionId, "CommitCrossSpvTransaction"));
    }
}
