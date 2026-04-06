namespace Whycespace.Engines.T0U.WhycePolicy.Classification;

/// <summary>
/// T0U engine for classifying command types as Tier-0 (requiring guardian approval).
/// Stateless utility — pure classification logic with no domain imports.
/// </summary>
public static class Tier0ActionClassifier
{
    /// <summary>
    /// Determines whether a command type is a Tier-0 protected action
    /// that requires multi-party guardian approval before execution.
    /// </summary>
    public static bool IsTier0Action(string commandType) =>
        commandType.StartsWith("policy.modify", StringComparison.OrdinalIgnoreCase) ||
        commandType.StartsWith("system.config", StringComparison.OrdinalIgnoreCase) ||
        commandType.StartsWith("economic.rule", StringComparison.OrdinalIgnoreCase) ||
        commandType.StartsWith("governance.override", StringComparison.OrdinalIgnoreCase);
}
