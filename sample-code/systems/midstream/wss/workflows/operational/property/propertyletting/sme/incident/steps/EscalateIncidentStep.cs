namespace Whycespace.Systems.Midstream.Wss.Workflows.Operational.Property.PropertyLetting.Sme.Incident;

public sealed class EscalateIncidentStep : IIncidentWorkflowStep
{
    public Task<IncidentStepResult> ExecuteAsync(
        IncidentStepCommand command,
        CancellationToken ct = default)
    {
        if (command is not EscalateIncidentStepCommand)
            return Task.FromResult(IncidentStepResult.Fail(
                command.IncidentId, nameof(EscalateIncidentStep), "Invalid command type."));

        return Task.FromResult(IncidentStepResult.Ok(
            command.IncidentId, nameof(EscalateIncidentStep)));
    }
}
