namespace Whycespace.Systems.Midstream.Wss.Workflows.Operational.Property.PropertyLetting.Sme.Incident;

/// <summary>
/// Declarative step definition for incident creation.
/// Defines step metadata ONLY — NO execution logic, NO domain references.
/// Execution is handled by T1M step executors via runtime.
/// </summary>
public sealed class CreateIncidentStep : IIncidentWorkflowStep
{
    public Task<IncidentStepResult> ExecuteAsync(
        IncidentStepCommand command,
        CancellationToken ct = default)
    {
        if (command is not CreateIncidentStepCommand)
            return Task.FromResult(IncidentStepResult.Fail(
                command.IncidentId, nameof(CreateIncidentStep), "Invalid command type."));

        // Declarative: validate step input, delegate execution to T1M
        return Task.FromResult(IncidentStepResult.Ok(command.IncidentId, nameof(CreateIncidentStep)));
    }
}
