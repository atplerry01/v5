namespace Whycespace.Engines.T0U.WhycePolicy.Impact;

/// <summary>
/// Assessment of a policy change impact.
/// Used to understand the blast radius of rule modifications.
/// </summary>
public sealed record PolicyImpactAssessment(
    string PolicyName,
    string[] AffectedRules,
    int EstimatedAffectedCommands,
    string ImpactHash,
    string ImpactLevel);
