namespace Whycespace.Domain.ConstitutionalSystem.Policy.Jurisdiction;

/// <summary>
/// Strategy for resolving conflicts when multiple jurisdiction overlays apply.
/// </summary>
public sealed record ConflictResolutionStrategy
{
    public static readonly ConflictResolutionStrategy MostRestrictive = new("MostRestrictive");
    public static readonly ConflictResolutionStrategy HighestPriority = new("HighestPriority");
    public static readonly ConflictResolutionStrategy LocalOverridesGlobal = new("LocalOverridesGlobal");

    public string Value { get; }
    private ConflictResolutionStrategy(string value) => Value = value;
}
