namespace Whycespace.Systems.Midstream.Wss.Workflows.Operational.Property.PropertyLetting.Sme.Incident;

public sealed class SLACheckStep : IIncidentWorkflowStep
{
    public Task<IncidentStepResult> ExecuteAsync(
        IncidentStepCommand command,
        CancellationToken ct = default)
    {
        if (command is not CheckSLAStepCommand)
            return Task.FromResult(IncidentStepResult.Fail(
                command.IncidentId, nameof(SLACheckStep), "Invalid command type."));

        return Task.FromResult(IncidentStepResult.Ok(
            command.IncidentId, nameof(SLACheckStep)));
    }
}
