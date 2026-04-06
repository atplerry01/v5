namespace Whycespace.Systems.Midstream.Wss.Workflows.Operational.Property.PropertyLetting.Sme.Incident;

public sealed class IncidentEscalationPolicy
{
    public static readonly int MaxEscalationLevel = 3;

    public bool ShouldEscalate(int currentLevel, bool slaBreach, bool severityRuleTriggered)
    {
        if (currentLevel >= MaxEscalationLevel)
            return false;

        return slaBreach || severityRuleTriggered;
    }

    public int NextEscalationLevel(int currentLevel)
    {
        if (currentLevel >= MaxEscalationLevel)
            return currentLevel;

        return currentLevel + 1;
    }

    public string GetEscalationTarget(int level) => level switch
    {
        1 => "Operator",
        2 => "Supervisor",
        3 => "Governance",
        _ => "Governance"
    };
}
