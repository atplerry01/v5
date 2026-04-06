namespace Whycespace.Domain.EconomicSystem.Enforcement.Enforcement;

public sealed class EnforcementAction : Entity
{
    public Guid IdentityId { get; private set; }
    public EnforcementType Type { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public DateTimeOffset AppliedAt { get; private set; }

    public static EnforcementAction Create(Guid id, Guid identityId, EnforcementType type, string reason, DateTimeOffset timestamp)
    {
        return new EnforcementAction
        {
            Id = id,
            IdentityId = identityId,
            Type = type,
            Reason = reason,
            AppliedAt = timestamp
        };
    }
}
