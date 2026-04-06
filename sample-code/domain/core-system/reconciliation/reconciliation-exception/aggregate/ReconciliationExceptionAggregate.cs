using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.CoreSystem.Reconciliation.ReconciliationException;

public sealed class ReconciliationExceptionAggregate : AggregateRoot
{
    public string Description { get; private set; } = string.Empty;
    public string Severity { get; private set; } = string.Empty;
    public bool IsResolved { get; private set; }
    public string? Resolution { get; private set; }

    public static ReconciliationExceptionAggregate Create(Guid id, string description, string severity)
    {
        var agg = new ReconciliationExceptionAggregate
        {
            Id = id,
            Description = description,
            Severity = severity,
            IsResolved = false
        };
        agg.RaiseDomainEvent(new ReconciliationExceptionCreatedEvent(id, description, severity));
        return agg;
    }

    public void Resolve(string resolution)
    {
        IsResolved = true;
        Resolution = resolution;
        RaiseDomainEvent(new ReconciliationExceptionResolvedEvent(Id, resolution));
    }
}
