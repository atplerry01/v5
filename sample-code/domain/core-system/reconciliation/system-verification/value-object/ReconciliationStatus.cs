using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.CoreSystem.Reconciliation.SystemVerification;

/// <summary>
/// Status of a system verification session.
/// </summary>
public sealed class ReconciliationStatus : ValueObject
{
    public static readonly ReconciliationStatus Pending = new("Pending");
    public static readonly ReconciliationStatus InProgress = new("InProgress");
    public static readonly ReconciliationStatus Verified = new("Verified");
    public static readonly ReconciliationStatus Failed = new("Failed");

    public string Value { get; }

    private ReconciliationStatus(string value) => Value = value;

    public bool IsTerminal => this == Verified || this == Failed;

    public static bool IsValidTransition(ReconciliationStatus from, ReconciliationStatus to) =>
        (from, to) switch
        {
            _ when from == Pending && to == InProgress => true,
            _ when from == InProgress && to == Verified => true,
            _ when from == InProgress && to == Failed => true,
            _ => false
        };

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
