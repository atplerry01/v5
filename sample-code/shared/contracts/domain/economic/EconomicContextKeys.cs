namespace Whycespace.Shared.Contracts.Domain.Economic;

/// <summary>
/// Well-known context property keys for economic data stored in CommandContext.Properties.
/// Used by economic middleware to enrich and by EconomicExecutionContext to read.
/// </summary>
public static class EconomicContextKeys
{
    public const string IdentityId = "Economic.IdentityId";
    public const string TrustScore = "Economic.TrustScore";
    public const string FederationStatus = "Economic.FederationStatus";

    public const string PolicyDecision = "Economic.PolicyDecision";
    public const string PolicyReasonCode = "Economic.PolicyReasonCode";
    public const string PolicyConditions = "Economic.PolicyConditions";
    public const string PolicyDecisionSealed = "Economic.PolicyDecisionSealed";

    public const string EnforcementDecision = "Economic.EnforcementDecision";
    public const string EnforcementReasonCode = "Economic.EnforcementReasonCode";

    public const string ConditionalRequiredActions = "Economic.ConditionalRequiredActions";

    public const string LedgerCorrelationId = "Economic.LedgerCorrelationId";
    public const string LedgerEntryRecorded = "Economic.LedgerEntryRecorded";
    public const string RequiresLedgerEntry = "Economic.RequiresLedgerEntry";
}

/// <summary>
/// Strong-typed enforcement decision. Replaces boolean IsEnforcementClear.
/// </summary>
public enum EnforcementDecisionType
{
    None = 0,
    Allow = 1,
    Deny = 2,
    Conditional = 3
}
