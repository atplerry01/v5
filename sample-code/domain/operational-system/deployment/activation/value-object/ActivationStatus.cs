using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.OperationalSystem.Deployment.Activation;

public sealed class ActivationStatus : ValueObject
{
    public static readonly ActivationStatus Inactive = new("Inactive");
    public static readonly ActivationStatus Canary = new("Canary");
    public static readonly ActivationStatus Active = new("Active");
    public static readonly ActivationStatus Halted = new("Halted");
    public static readonly ActivationStatus Decommissioned = new("Decommissioned");

    public string Value { get; }
    private ActivationStatus(string value) => Value = value;
    public bool IsTerminal => this == Decommissioned;
    public bool IsOperational => this == Canary || this == Active;

    public static bool IsValidTransition(ActivationStatus from, ActivationStatus to) =>
        (from, to) switch
        {
            _ when from == Inactive && to == Canary => true,
            _ when from == Canary && to == Active => true,
            _ when from == Canary && to == Halted => true,
            _ when from == Active && to == Halted => true,
            _ when from == Halted && to == Canary => true,
            _ when from == Halted && to == Decommissioned => true,
            _ => false
        };

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
