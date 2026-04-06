using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.StructuralSystem.HumanCapital.Sanction;

public sealed class SanctionAggregate : AggregateRoot
{
    public SanctionType? Type { get; private set; }
    public SanctionDuration Duration { get; private set; } = new(TimeSpan.Zero);
    public bool IsActive { get; private set; }

    public static SanctionAggregate Create(Guid sanctionId, Guid typeId, string typeName, string severity, TimeSpan duration)
    {
        Guard.AgainstDefault(sanctionId);
        Guard.AgainstDefault(typeId);
        Guard.AgainstEmpty(typeName);
        Guard.AgainstEmpty(severity);
        Guard.AgainstInvalid(duration, d => d > TimeSpan.Zero, "Duration must be greater than zero.");

        var sanction = new SanctionAggregate();
        sanction.Type = new SanctionType
        {
            Id = typeId,
            TypeName = typeName,
            Severity = severity
        };
        sanction.Apply(new SanctionAppliedEvent(sanctionId, typeName, duration));
        return sanction;
    }

    public void Apply(string sanctionType, TimeSpan duration)
    {
        Guard.AgainstEmpty(sanctionType);
        Guard.AgainstInvalid(duration, d => d > TimeSpan.Zero, "Duration must be greater than zero.");
        EnsureInvariant(!IsActive, "ALREADY_ACTIVE", "Sanction is already active.");

        Apply(new SanctionAppliedEvent(Id, sanctionType, duration));
    }

    public void Lift()
    {
        EnsureInvariant(IsActive, "ALREADY_LIFTED", "Sanction is already lifted.");

        Apply(new SanctionLiftedEvent(Id));
    }

    private void Apply(SanctionAppliedEvent e)
    {
        Id = e.SanctionId;
        Duration = new SanctionDuration(e.Duration);
        IsActive = true;
        RaiseDomainEvent(e);
    }

    private void Apply(SanctionLiftedEvent e)
    {
        IsActive = false;
        RaiseDomainEvent(e);
    }
}
