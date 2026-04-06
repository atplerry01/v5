namespace Whycespace.Systems.Midstream.Wss.Workflows.Operational.Property.PropertyLetting.Sme.Incident;

public sealed class CloseIncidentStep : IIncidentWorkflowStep
{
    public Task<IncidentStepResult> ExecuteAsync(
        IncidentStepCommand command,
        CancellationToken ct = default)
    {
        if (command is not CloseIncidentStepCommand)
            return Task.FromResult(IncidentStepResult.Fail(
                command.IncidentId, nameof(CloseIncidentStep), "Invalid command type."));

        return Task.FromResult(IncidentStepResult.Ok(
            command.IncidentId, nameof(CloseIncidentStep)));
    }
}
