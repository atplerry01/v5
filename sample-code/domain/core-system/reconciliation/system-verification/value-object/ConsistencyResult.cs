namespace Whycespace.Domain.CoreSystem.Reconciliation.SystemVerification;

/// <summary>
/// Outcome of a single consistency check within a verification session.
/// </summary>
public sealed record ConsistencyResult
{
    public bool IsConsistent { get; }
    public string CheckName { get; }
    public string Details { get; }

    private ConsistencyResult(bool isConsistent, string checkName, string details)
    {
        IsConsistent = isConsistent;
        CheckName = checkName;
        Details = details;
    }

    public static ConsistencyResult Pass(string checkName) =>
        new(true, checkName, "Consistent");

    public static ConsistencyResult Fail(string checkName, string details) =>
        new(false, checkName, details);
}
