namespace Whycespace.Systems.Midstream.Wss.Workflows.Operational.Property.PropertyLetting.Sme.Incident;

public sealed class ResolveIncidentStep : IIncidentWorkflowStep
{
    public Task<IncidentStepResult> ExecuteAsync(
        IncidentStepCommand command,
        CancellationToken ct = default)
    {
        if (command is not ResolveIncidentStepCommand)
            return Task.FromResult(IncidentStepResult.Fail(
                command.IncidentId, nameof(ResolveIncidentStep), "Invalid command type."));

        return Task.FromResult(IncidentStepResult.Ok(
            command.IncidentId, nameof(ResolveIncidentStep)));
    }
}
