namespace Whycespace.Domain.EconomicSystem.Transaction.Charge;

public sealed record ChargeStatus(string Value)
{
    public static readonly ChargeStatus Created = new("created");
    public static readonly ChargeStatus Approved = new("approved");
    public static readonly ChargeStatus Applied = new("applied");
    public static readonly ChargeStatus Waived = new("waived");
    public static readonly ChargeStatus Reversed = new("reversed");

    public bool IsTerminal => this == Waived || this == Reversed;

    /// <summary>
    /// Full lifecycle: Created -> Approved -> Applied -> Waived | Reversed
    /// Waived can happen from Created or Approved (before application).
    /// Reversed can only happen after Applied.
    /// </summary>
    public static readonly Dictionary<ChargeStatus, HashSet<ChargeStatus>> ValidTransitions = new()
    {
        [Created] = [Approved, Waived],
        [Approved] = [Applied, Waived],
        [Applied] = [Reversed],
        [Waived] = [],
        [Reversed] = []
    };

    public bool CanTransitionTo(ChargeStatus target) =>
        ValidTransitions.TryGetValue(this, out var allowed) && allowed.Contains(target);

    public override string ToString() => Value;
}
