namespace Whycespace.Domain.ConstitutionalSystem.Policy.Violation;

using Whycespace.Domain.SharedKernel;

public sealed class ViolationStatus : ValueObject
{
    public string Value { get; }

    private ViolationStatus(string value) => Value = value;

    public static readonly ViolationStatus Detected = new("detected");
    public static readonly ViolationStatus Acknowledged = new("acknowledged");
    public static readonly ViolationStatus Escalated = new("escalated");
    public static readonly ViolationStatus Resolved = new("resolved");

    private static readonly Dictionary<string, ViolationStatus> All = new()
    {
        [Detected.Value] = Detected,
        [Acknowledged.Value] = Acknowledged,
        [Escalated.Value] = Escalated,
        [Resolved.Value] = Resolved
    };

    public static ViolationStatus From(string value)
    {
        if (!All.TryGetValue(value, out var status))
            throw new ArgumentException($"Unknown violation status: {value}");
        return status;
    }

    public bool CanTransitionTo(ViolationStatus target)
    {
        if (this == Detected) return target == Acknowledged || target == Escalated || target == Resolved;
        if (this == Acknowledged) return target == Escalated || target == Resolved;
        if (this == Escalated) return target == Resolved;
        return false;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
