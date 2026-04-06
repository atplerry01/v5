namespace Whycespace.Systems.Midstream.Wss.Workflows.Operational.Property.PropertyLetting.Sme.Incident;

public sealed class StartProgressStep : IIncidentWorkflowStep
{
    public Task<IncidentStepResult> ExecuteAsync(
        IncidentStepCommand command,
        CancellationToken ct = default)
    {
        if (command is not StartProgressStepCommand)
            return Task.FromResult(IncidentStepResult.Fail(
                command.IncidentId, nameof(StartProgressStep), "Invalid command type."));

        return Task.FromResult(IncidentStepResult.Ok(
            command.IncidentId, nameof(StartProgressStep)));
    }
}
