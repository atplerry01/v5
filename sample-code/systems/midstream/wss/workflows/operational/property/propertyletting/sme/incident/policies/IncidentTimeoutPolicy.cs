namespace Whycespace.Systems.Midstream.Wss.Workflows.Operational.Property.PropertyLetting.Sme.Incident;

public sealed class IncidentTimeoutPolicy
{
    private static readonly Dictionary<string, TimeSpan> StepTimeouts = new()
    {
        ["CreateIncident"] = TimeSpan.FromMinutes(1),
        ["AssignIncident"] = TimeSpan.FromMinutes(5),
        ["StartProgress"] = TimeSpan.FromMinutes(1),
        ["SLACheck"] = TimeSpan.FromMinutes(1),
        ["EscalateIncident"] = TimeSpan.FromMinutes(2),
        ["ResolveIncident"] = TimeSpan.FromMinutes(30),
        ["CloseIncident"] = TimeSpan.FromMinutes(1)
    };

    public TimeSpan GetTimeout(string stepName)
    {
        return StepTimeouts.TryGetValue(stepName, out var timeout)
            ? timeout
            : TimeSpan.FromMinutes(5);
    }
}
