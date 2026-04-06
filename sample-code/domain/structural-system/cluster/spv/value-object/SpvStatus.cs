namespace Whycespace.Domain.StructuralSystem.Cluster.Spv;

public sealed record SpvStatus(string Value)
{
    public static readonly SpvStatus Created = new("created");
    public static readonly SpvStatus Active = new("active");
    public static readonly SpvStatus Suspended = new("suspended");
    public static readonly SpvStatus Terminated = new("terminated");
    public static readonly SpvStatus Closed = new("closed");

    public bool IsTerminal => this == Closed || this == Terminated;

    /// <summary>
    /// Full lifecycle: Create → Activate → Operate → Suspend → Terminate → Close (Audit)
    /// Suspended can be reactivated. Terminated is pre-closure (pending audit). Closed is final.
    /// </summary>
    public static readonly Dictionary<SpvStatus, HashSet<SpvStatus>> ValidTransitions = new()
    {
        [Created] = [Active],
        [Active] = [Suspended, Terminated],
        [Suspended] = [Active, Terminated],
        [Terminated] = [Closed],
        [Closed] = []
    };

    public bool CanTransitionTo(SpvStatus target) =>
        ValidTransitions.TryGetValue(this, out var allowed) && allowed.Contains(target);

    public override string ToString() => Value;
}
