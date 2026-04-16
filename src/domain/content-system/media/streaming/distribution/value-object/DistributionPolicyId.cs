namespace Whycespace.Domain.ContentSystem.Media.Streaming.Distribution;

public readonly record struct DistributionPolicyId(Guid Value)
{
    public static DistributionPolicyId From(Guid value) => new(value);
    public override string ToString() => Value.ToString("D");
}
