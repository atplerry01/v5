namespace Whycespace.Domain.DecisionSystem.Governance.Guardian;

/// <summary>
/// Result of bias detection on a recommendation or recommendation set.
/// </summary>
public sealed record BiasAssessment
{
    public bool BiasDetected { get; }
    public BiasType Type { get; }
    public string Severity { get; }
    public string Description { get; }

    private BiasAssessment(bool detected, BiasType type, string severity, string description)
    {
        BiasDetected = detected;
        Type = type;
        Severity = severity;
        Description = description;
    }

    public static BiasAssessment None() =>
        new(false, BiasType.None, "None", "No bias detected.");

    public static BiasAssessment Detected(BiasType type, string severity, string description) =>
        new(true, type, severity, description);
}

public enum BiasType
{
    None,
    Repetition,
    Cluster,
    OutcomeSkew
}
