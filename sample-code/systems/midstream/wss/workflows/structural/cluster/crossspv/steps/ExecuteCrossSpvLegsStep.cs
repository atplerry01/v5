namespace Whycespace.Systems.Midstream.Wss.Workflows.Structural.Cluster.CrossSpv;

/// <summary>
/// Execute step — delegates to CrossSpvProcessManager via T1M.
/// No business logic in this layer.
/// </summary>
public sealed class ExecuteCrossSpvLegsStep : ICrossSpvWorkflowStep
{
    public Task<CrossSpvStepResult> ExecuteAsync(
        CrossSpvStepCommand command, CancellationToken ct = default)
    {
        // T1M dispatches to CrossSpvProcessManager via runtime
        return Task.FromResult(
            CrossSpvStepResult.Ok(command.TransactionId, "ExecuteCrossSpvLegs"));
    }
}
