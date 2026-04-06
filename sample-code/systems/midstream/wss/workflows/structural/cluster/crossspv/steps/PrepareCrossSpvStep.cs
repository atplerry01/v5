namespace Whycespace.Systems.Midstream.Wss.Workflows.Structural.Cluster.CrossSpv;

/// <summary>
/// Prepare step — declarative, delegates to T2E via T1M.
/// No business logic in this layer.
/// </summary>
public sealed class PrepareCrossSpvStep : ICrossSpvWorkflowStep
{
    public Task<CrossSpvStepResult> ExecuteAsync(
        CrossSpvStepCommand command, CancellationToken ct = default)
    {
        // T1M dispatches to T2E CrossSpvEngine via EngineInvocationDispatcher
        // This step is a declaration of intent — execution happens in the engine layer
        return Task.FromResult(
            CrossSpvStepResult.Ok(command.TransactionId, "PrepareCrossSpvTransaction"));
    }
}
