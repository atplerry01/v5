using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Scheduling.SystemJob;

public sealed class SystemJobAggregate : AggregateRoot
{
    public SystemJobId Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public JobCategory Category { get; private set; }
    public TimeSpan Timeout { get; private set; }
    public bool IsDeprecated { get; private set; }

    private SystemJobAggregate() { }

    public static SystemJobAggregate Define(SystemJobId id, string name, JobCategory category, TimeSpan timeout)
    {
        Guard.Against(string.IsNullOrEmpty(name), "SystemJob name must not be empty.");
        Guard.Against(timeout <= TimeSpan.Zero, "SystemJob timeout must be positive.");

        var aggregate = new SystemJobAggregate();
        aggregate.RaiseDomainEvent(new SystemJobDefinedEvent(id, name, category, timeout));
        return aggregate;
    }

    public void Deprecate()
    {
        Guard.Against(IsDeprecated, "SystemJob is already deprecated.");
        RaiseDomainEvent(new SystemJobDeprecatedEvent(Id));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case SystemJobDefinedEvent e: Id = e.Id; Name = e.Name; Category = e.Category; Timeout = e.Timeout; break;
            case SystemJobDeprecatedEvent: IsDeprecated = true; break;
        }
    }

    protected override void EnsureInvariants() =>
        Guard.Against(Id.Value is null, "SystemJob must have an Id.");
}
