namespace Whycespace.Domain.DecisionSystem.Compliance.Jurisdiction;

public sealed record JurisdictionBinding(
    Guid RegulationId,
    string BindingType,
    DateTimeOffset EffectiveFrom,
    DateTimeOffset? EffectiveUntil = null)
{
    public static readonly string Primary = "PRIMARY";
    public static readonly string Secondary = "SECONDARY";
    public static readonly string Advisory = "ADVISORY";

    public bool IsEffective(DateTimeOffset asOf) =>
        asOf >= EffectiveFrom && (EffectiveUntil is null || asOf <= EffectiveUntil);
}
