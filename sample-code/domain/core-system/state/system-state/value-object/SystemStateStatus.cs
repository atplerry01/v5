using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.CoreSystem.State.SystemState;

/// <summary>
/// Status of the system state authority.
/// </summary>
public sealed class SystemStateStatus : ValueObject
{
    public static readonly SystemStateStatus Initializing = new("Initializing");
    public static readonly SystemStateStatus Active = new("Active");
    public static readonly SystemStateStatus Validating = new("Validating");
    public static readonly SystemStateStatus Authoritative = new("Authoritative");
    public static readonly SystemStateStatus Degraded = new("Degraded");

    public string Value { get; }

    private SystemStateStatus(string value) => Value = value;

    public bool IsTerminal => this == Authoritative;

    public static bool IsValidTransition(SystemStateStatus from, SystemStateStatus to) =>
        (from, to) switch
        {
            _ when from == Initializing && to == Active => true,
            _ when from == Active && to == Validating => true,
            _ when from == Validating && to == Authoritative => true,
            _ when from == Validating && to == Degraded => true,
            _ when from == Degraded && to == Validating => true,
            _ => false
        };

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
