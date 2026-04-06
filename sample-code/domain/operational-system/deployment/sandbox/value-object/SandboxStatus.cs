using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.OperationalSystem.Deployment.Sandbox;

public sealed class SandboxStatus : ValueObject
{
    public static readonly SandboxStatus Created = new("Created");
    public static readonly SandboxStatus Active = new("Active");
    public static readonly SandboxStatus Closed = new("Closed");

    public string Value { get; }
    private SandboxStatus(string value) => Value = value;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
