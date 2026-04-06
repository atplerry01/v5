namespace Whycespace.Domain.OperationalSystem.IncidentResponse.Incident;

public sealed class IncidentAggregate : AggregateRoot
{
    public string IncidentCategory { get; private set; } = string.Empty;
    public Guid AffectedEntityId { get; private set; }

    public static IncidentAggregate Initiate(Guid id, string category, Guid affectedEntityId)
    {
        var aggregate = new IncidentAggregate();
        aggregate.Id = id;
        aggregate.IncidentCategory = category;
        aggregate.AffectedEntityId = affectedEntityId;
        aggregate.RaiseDomainEvent(new IncidentInitiatedEvent(id, category, affectedEntityId));
        return aggregate;
    }
}
