using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Schema.Versioning;

public readonly record struct VersioningRuleId
{
    public Guid Value { get; }

    public VersioningRuleId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "VersioningRuleId cannot be empty.");
        Value = value;
    }
}
