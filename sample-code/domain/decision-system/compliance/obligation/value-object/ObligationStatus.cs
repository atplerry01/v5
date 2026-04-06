namespace Whycespace.Domain.DecisionSystem.Compliance.Obligation;

public sealed record ObligationStatus(string Value)
{
    public static readonly ObligationStatus Pending = new("PENDING");
    public static readonly ObligationStatus Assigned = new("ASSIGNED");
    public static readonly ObligationStatus InProgress = new("IN_PROGRESS");
    public static readonly ObligationStatus Fulfilled = new("FULFILLED");
    public static readonly ObligationStatus Breached = new("BREACHED");
    public static readonly ObligationStatus Waived = new("WAIVED");

    public bool IsTerminal => this == Fulfilled || this == Breached || this == Waived;
}
