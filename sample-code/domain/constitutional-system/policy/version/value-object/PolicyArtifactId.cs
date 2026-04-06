namespace Whycespace.Domain.ConstitutionalSystem.Policy.Version;

using Whycespace.Domain.SharedKernel;

public sealed class PolicyArtifactId : ValueObject
{
    public string Value { get; }

    private PolicyArtifactId(string value) => Value = value;

    public static PolicyArtifactId Generate(Guid policyId, int versionNumber)
    {
        return new PolicyArtifactId($"{policyId}:{versionNumber}");
    }

    public static PolicyArtifactId From(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        return new PolicyArtifactId(value);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
