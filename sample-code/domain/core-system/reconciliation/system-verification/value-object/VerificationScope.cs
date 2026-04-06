namespace Whycespace.Domain.CoreSystem.Reconciliation.SystemVerification;

/// <summary>
/// Defines what is being verified: event-store vs projection, cross-ledger, or SPV correctness.
/// </summary>
public sealed record VerificationScope
{
    public string ScopeType { get; }
    public string TargetSystem { get; }

    private VerificationScope(string scopeType, string targetSystem)
    {
        ScopeType = scopeType;
        TargetSystem = targetSystem;
    }

    public static VerificationScope EventStoreVsProjection(string targetSystem) =>
        new("EventStoreVsProjection", targetSystem);

    public static VerificationScope CrossLedger(string targetSystem) =>
        new("CrossLedger", targetSystem);

    public static VerificationScope SpvFinancial(string targetSystem) =>
        new("SpvFinancial", targetSystem);
}
