namespace Whycespace.Domain.EconomicSystem.Revenue.Payout;

public sealed record PayoutStatus(string Value)
{
    public static readonly PayoutStatus Scheduled = new("scheduled");
    public static readonly PayoutStatus Approved = new("approved");
    public static readonly PayoutStatus Processing = new("processing");
    public static readonly PayoutStatus Completed = new("completed");
    public static readonly PayoutStatus Failed = new("failed");
    public static readonly PayoutStatus Held = new("held");

    public bool IsTerminal => this == Completed || this == Failed;

    /// <summary>
    /// Payout lifecycle: Schedule → Approve → Process → Complete/Fail
    /// Scheduled/Approved can be held. Held can be released back to Approved.
    /// Processing resolves to Completed or Failed. Both are terminal.
    /// </summary>
    public static readonly Dictionary<PayoutStatus, HashSet<PayoutStatus>> ValidTransitions = new()
    {
        [Scheduled] = [Approved, Held],
        [Approved] = [Processing, Held],
        [Processing] = [Completed, Failed],
        [Completed] = [],
        [Failed] = [],
        [Held] = [Approved]
    };

    public bool CanTransitionTo(PayoutStatus target) =>
        ValidTransitions.TryGetValue(this, out var allowed) && allowed.Contains(target);

    public override string ToString() => Value;
}
