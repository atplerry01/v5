using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Cluster.Authority;

public readonly record struct AuthorityId
{
    public Guid Value { get; }

    public AuthorityId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "AuthorityId cannot be empty.");
        Value = value;
    }
}
