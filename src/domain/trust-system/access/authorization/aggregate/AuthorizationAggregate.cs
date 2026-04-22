using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.TrustSystem.Access.Authorization;

public sealed class AuthorizationAggregate : AggregateRoot
{
    public AuthorizationId Id { get; private set; }
    public AuthorizationScope Scope { get; private set; }
    public AuthorizationStatus Status { get; private set; }

    private AuthorizationAggregate() { }

    // ── Factory: Grant ──────────────────────────────────────────

    public static AuthorizationAggregate Grant(
        AuthorizationId id,
        AuthorizationScope scope)
    {
        var aggregate = new AuthorizationAggregate();
        aggregate.RaiseDomainEvent(new AuthorizationGrantedEvent(id, scope));
        return aggregate;
    }

    // ── Factory: Deny ───────────────────────────────────────────

    public static AuthorizationAggregate Deny(
        AuthorizationId id,
        AuthorizationScope scope)
    {
        var aggregate = new AuthorizationAggregate();
        aggregate.RaiseDomainEvent(new AuthorizationDeniedEvent(id, scope));
        return aggregate;
    }

    // ── Transition: Revoke ──────────────────────────────────────

    public void Revoke()
    {
        var specification = new CanRevokeSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw AuthorizationErrors.InvalidStateTransition(Status, nameof(Revoke));

        RaiseDomainEvent(new AuthorizationRevokedEvent(Id));
    }

    // ── Apply ───────────────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case AuthorizationGrantedEvent e:
                Id = e.AuthorizationId;
                Scope = e.Scope;
                Status = AuthorizationStatus.Granted;
                break;
            case AuthorizationDeniedEvent e:
                Id = e.AuthorizationId;
                Scope = e.Scope;
                Status = AuthorizationStatus.Denied;
                break;
            case AuthorizationRevokedEvent:
                Status = AuthorizationStatus.Revoked;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw AuthorizationErrors.MissingId();

        if (Scope == default)
            throw AuthorizationErrors.MissingScope();

        if (!Enum.IsDefined(Status))
            throw AuthorizationErrors.InvalidStateTransition(Status, "validate");
    }
}
