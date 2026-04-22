namespace Whycespace.Domain.ControlSystem.Enforcement.Escalation;

/// <summary>
/// Pure predicate mapping the accumulated counter to an escalation level.
/// Thresholds are intentionally conservative and stable — the canonical
/// authority for sanction *behavior* remains WHYCEPOLICY; this specification
/// only classifies severity-weighted accumulation into a level bucket so the
/// aggregate can emit a single categorical event per transition.
/// </summary>
public sealed class EscalationThresholdSpecification
{
    public const int WeightLow      = 1;
    public const int WeightMedium   = 3;
    public const int WeightHigh     = 7;
    public const int WeightCritical = 15;

    public const int ScoreLow      = 3;
    public const int ScoreMedium   = 10;
    public const int ScoreHigh     = 25;
    public const int ScoreCritical = 50;

    public static int WeightFor(string severity) => severity switch
    {
        "Low"      => WeightLow,
        "Medium"   => WeightMedium,
        "High"     => WeightHigh,
        "Critical" => WeightCritical,
        _          => 0
    };

    public EscalationLevel Classify(ViolationCounter counter)
    {
        var s = counter.SeverityScore;
        if (s >= ScoreCritical) return EscalationLevel.Critical;
        if (s >= ScoreHigh)     return EscalationLevel.High;
        if (s >= ScoreMedium)   return EscalationLevel.Medium;
        if (s >= ScoreLow)      return EscalationLevel.Low;
        return EscalationLevel.None;
    }
}
