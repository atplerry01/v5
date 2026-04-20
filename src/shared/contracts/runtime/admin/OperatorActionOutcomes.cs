namespace Whycespace.Shared.Contracts.Runtime.Admin;

/// <summary>
/// R4.B — canonical outcome discriminators for
/// <see cref="IOperatorActionRecorder.RecordAsync"/>. Using constants
/// instead of inline strings at call sites prevents accidental divergence
/// between "Accepted" / "accepted" / "ok" across controllers.
/// </summary>
public static class OperatorActionOutcomes
{
    public const string Accepted = "accepted";
    public const string Refused = "refused";
    public const string Failed = "failed";
}
