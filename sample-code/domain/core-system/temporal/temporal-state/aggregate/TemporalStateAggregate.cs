using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.CoreSystem.Temporal.TemporalState;

public sealed class TemporalStateAggregate : AggregateRoot
{
    public Guid EntityId { get; private set; }
    public DateTimeOffset EffectiveFrom { get; private set; }
    public bool IsSuperseded { get; private set; }

    public static TemporalStateAggregate Create(Guid id, Guid entityId, DateTimeOffset effectiveFrom)
    {
        var agg = new TemporalStateAggregate
        {
            Id = id,
            EntityId = entityId,
            EffectiveFrom = effectiveFrom,
            IsSuperseded = false
        };
        agg.RaiseDomainEvent(new TemporalStateCreatedEvent(id, entityId, effectiveFrom));
        return agg;
    }

    public void Supersede(DateTimeOffset newEffectiveFrom)
    {
        IsSuperseded = true;
        RaiseDomainEvent(new TemporalStateSupersededEvent(Id, newEffectiveFrom));
    }
}
