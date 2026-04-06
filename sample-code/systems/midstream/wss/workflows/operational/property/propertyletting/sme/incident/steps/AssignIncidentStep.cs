namespace Whycespace.Systems.Midstream.Wss.Workflows.Operational.Property.PropertyLetting.Sme.Incident;

public sealed class AssignIncidentStep : IIncidentWorkflowStep
{
    public Task<IncidentStepResult> ExecuteAsync(
        IncidentStepCommand command,
        CancellationToken ct = default)
    {
        if (command is not AssignIncidentStepCommand)
            return Task.FromResult(IncidentStepResult.Fail(
                command.IncidentId, nameof(AssignIncidentStep), "Invalid command type."));

        return Task.FromResult(IncidentStepResult.Ok(
            command.IncidentId, nameof(AssignIncidentStep)));
    }
}
