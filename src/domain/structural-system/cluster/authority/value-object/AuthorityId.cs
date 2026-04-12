namespace Whycespace.Domain.StructuralSystem.Cluster.Authority;

public readonly record struct AuthorityId
{
    public Guid Value { get; }

    public AuthorityId(Guid value)
    {
        if (value == Guid.Empty)
            throw AuthorityErrors.MissingId();

        Value = value;
    }
}
