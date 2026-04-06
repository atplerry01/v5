using Whycespace.Domain.SharedKernel;
using Whycespace.Domain.SharedKernel.Primitive.Identity;
using Whycespace.Shared.Primitives.Id;

namespace Whycespace.Domain.StructuralSystem.HumanCapital.Operator;

public sealed class OperatorAggregate : AggregateRoot
{
    public OperatorId OperatorId { get; private set; } = new(Guid.Empty);
    public IdentityId IdentityId { get; private set; } = default!;
    public OperatorRole? Role { get; private set; }
    public Certification? Certification { get; private set; }
    public AuthorizationLevel AuthorizationLevel { get; private set; } = AuthorizationLevel.Standard;
    public bool IsAuthorized { get; private set; }

    public static OperatorAggregate Create(Guid operatorId, IdentityId identityId, Guid roleId, string roleName, Guid certificationId, string certificationType, DateTimeOffset issuedAt)
    {
        Guard.AgainstDefault(operatorId);
        Guard.AgainstNull(identityId);
        Guard.AgainstDefault(roleId);
        Guard.AgainstEmpty(roleName);
        Guard.AgainstDefault(certificationId);
        Guard.AgainstEmpty(certificationType);

        var op = new OperatorAggregate();
        op.OperatorId = new OperatorId(operatorId);
        op.Id = operatorId;
        op.IdentityId = identityId;
        op.Role = new OperatorRole { Id = roleId, RoleName = roleName };
        op.Certification = new Certification { Id = certificationId, CertificationType = certificationType, IssuedAt = issuedAt };
        op.IsAuthorized = false;
        op.AuthorizationLevel = AuthorizationLevel.Standard;
        return op;
    }

    public void Authorize(string authorizationLevel)
    {
        Guard.AgainstEmpty(authorizationLevel);
        EnsureInvariant(!IsAuthorized, "ALREADY_AUTHORIZED", "Operator is already authorized.");

        Apply(new OperatorAuthorizedEvent(Id, authorizationLevel));
    }

    public void Revoke(string reason)
    {
        Guard.AgainstEmpty(reason);
        EnsureInvariant(IsAuthorized, "NOT_AUTHORIZED", "Operator is not authorized.");

        Apply(new OperatorRevokedEvent(Id, reason));
    }

    public void AssignRole(string roleName)
    {
        Guard.AgainstEmpty(roleName);

        Apply(new OperatorRoleAssignedEvent(Id, DeterministicIdHelper.FromSeed($"OperatorRole:{Id}:{roleName}"), roleName));
    }

    private void Apply(OperatorAuthorizedEvent e)
    {
        IsAuthorized = true;
        AuthorizationLevel = new AuthorizationLevel(e.AuthorizationLevel);
        RaiseDomainEvent(e);
    }

    private void Apply(OperatorRevokedEvent e)
    {
        IsAuthorized = false;
        AuthorizationLevel = AuthorizationLevel.Standard;
        RaiseDomainEvent(e);
    }

    private void Apply(OperatorRoleAssignedEvent e)
    {
        Role = new OperatorRole { Id = e.RoleId, RoleName = e.RoleName };
        RaiseDomainEvent(e);
    }
}
