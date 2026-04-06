namespace Whycespace.Domain.ConstitutionalSystem.Policy.Registry;

using Whycespace.Domain.SharedKernel;

public sealed class FederationNodeStatus : ValueObject
{
    public string Value { get; }

    private FederationNodeStatus(string value) => Value = value;

    public static readonly FederationNodeStatus Active = new("active");
    public static readonly FederationNodeStatus Inactive = new("inactive");
    public static readonly FederationNodeStatus Conflict = new("conflict");

    private static readonly Dictionary<string, FederationNodeStatus> All = new()
    {
        [Active.Value] = Active,
        [Inactive.Value] = Inactive,
        [Conflict.Value] = Conflict
    };

    public static FederationNodeStatus From(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        if (!All.TryGetValue(value, out var status))
            throw new ArgumentException($"Invalid federation node status: {value}");
        return status;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
