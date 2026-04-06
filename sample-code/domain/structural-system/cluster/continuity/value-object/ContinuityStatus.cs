namespace Whycespace.Domain.StructuralSystem.Cluster.Continuity;

public sealed record ContinuityStatus(string Value)
{
    public static readonly ContinuityStatus Draft = new("draft");
    public static readonly ContinuityStatus Active = new("active");
    public static readonly ContinuityStatus Triggered = new("triggered");
    public static readonly ContinuityStatus Recovering = new("recovering");
    public static readonly ContinuityStatus Completed = new("completed");
    public static readonly ContinuityStatus Failed = new("failed");

    public bool IsTerminal => this == Completed || this == Failed;

    /// <summary>
    /// Full lifecycle: Draft → Active → Triggered → Recovering → Completed/Failed
    /// Draft: plan under construction. Active: ready for failover trigger.
    /// Triggered: failover initiated. Recovering: recovery steps executing.
    /// Completed/Failed: terminal states.
    /// </summary>
    public static readonly Dictionary<ContinuityStatus, HashSet<ContinuityStatus>> ValidTransitions = new()
    {
        [Draft] = [Active],
        [Active] = [Triggered],
        [Triggered] = [Recovering],
        [Recovering] = [Completed, Failed],
        [Completed] = [],
        [Failed] = []
    };

    public bool CanTransitionTo(ContinuityStatus target) =>
        ValidTransitions.TryGetValue(this, out var allowed) && allowed.Contains(target);

    public override string ToString() => Value;
}
