using DomainEntity = Whycespace.Domain.SharedKernel.Primitives.Kernel.Entity;

namespace Whycespace.Domain.StructuralSystem.Cluster.Authority;

public sealed class AuthorityAssignment : DomainEntity
{
    public Guid AuthorityId { get; private set; }
    public Guid AssigneeId { get; private set; }
    public string Role { get; private set; } = string.Empty;
    public DateTimeOffset AssignedAt { get; private set; }
    public bool IsActive { get; private set; }

    private AuthorityAssignment() { }

    public static AuthorityAssignment Create(
        Guid id,
        Guid authorityId,
        Guid assigneeId,
        string role,
        DateTimeOffset timestamp)
    {
        return new AuthorityAssignment
        {
            Id = id,
            AuthorityId = authorityId,
            AssigneeId = assigneeId,
            Role = role,
            AssignedAt = timestamp,
            IsActive = true
        };
    }

    public void Revoke()
    {
        IsActive = false;
    }
}
