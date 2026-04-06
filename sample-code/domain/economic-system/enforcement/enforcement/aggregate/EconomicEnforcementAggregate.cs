namespace Whycespace.Domain.EconomicSystem.Enforcement.Enforcement;

public class EconomicEnforcementAggregate : AggregateRoot
{
    private IdentityId _identityId = null!;
    private EnforcementType _type;
    private EnforcementScope _scope;
    private EnforcementDuration _duration;
    private Reason _reason = null!;
    private bool _isActive;

    public void Apply(Guid id, IdentityId identityId, EnforcementType type, Reason reason, EnforcementScope scope = EnforcementScope.Identity, EnforcementDuration duration = EnforcementDuration.Temporary)
    {
        EnsureInvariant(id != Guid.Empty, "EnforcementId", "EnforcementId cannot be empty.");
        ArgumentNullException.ThrowIfNull(identityId);
        ArgumentNullException.ThrowIfNull(reason);

        Id = id;
        _identityId = identityId;
        _type = type;
        _scope = scope;
        _duration = duration;
        _reason = reason;
        _isActive = true;

        RaiseDomainEvent(new EnforcementAppliedEvent(id, identityId.Value, type.ToString(), reason.Value));
    }

    public void Release()
    {
        EnsureInvariant(_isActive, "EnforcementActive", "Only active enforcements can be released.");

        _isActive = false;
        RaiseDomainEvent(new EnforcementReleasedEvent(Id, _identityId.Value));
    }
}
