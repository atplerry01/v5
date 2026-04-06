using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.OperationalSystem.Deployment.Emergency;

public sealed class EmergencyStatus : ValueObject
{
    public static readonly EmergencyStatus Standby = new("Standby");
    public static readonly EmergencyStatus Active = new("Active");
    public static readonly EmergencyStatus Resolved = new("Resolved");

    public string Value { get; }
    private EmergencyStatus(string value) => Value = value;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
