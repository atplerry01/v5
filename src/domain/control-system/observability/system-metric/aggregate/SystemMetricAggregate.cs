using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Observability.SystemMetric;

public sealed class SystemMetricAggregate : AggregateRoot
{
    public SystemMetricId Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public MetricType Type { get; private set; }
    public string Unit { get; private set; } = string.Empty;
    public IReadOnlyList<string> LabelNames { get; private set; } = [];
    public bool IsDeprecated { get; private set; }

    private SystemMetricAggregate() { }

    public static SystemMetricAggregate Define(SystemMetricId id, string name, MetricType type, string unit, IEnumerable<string> labelNames)
    {
        Guard.Against(string.IsNullOrEmpty(name), "SystemMetric name must not be empty.");
        Guard.Against(string.IsNullOrEmpty(unit), "SystemMetric unit must not be empty.");
        var aggregate = new SystemMetricAggregate();
        aggregate.RaiseDomainEvent(new SystemMetricDefinedEvent(id, name, type, unit, labelNames.ToList()));
        return aggregate;
    }

    public void Deprecate()
    {
        Guard.Against(IsDeprecated, "SystemMetric is already deprecated.");
        RaiseDomainEvent(new SystemMetricDeprecatedEvent(Id));
    }

    protected override void Apply(object e)
    {
        switch (e)
        {
            case SystemMetricDefinedEvent ev: Id = ev.Id; Name = ev.Name; Type = ev.Type; Unit = ev.Unit; LabelNames = ev.LabelNames; break;
            case SystemMetricDeprecatedEvent: IsDeprecated = true; break;
        }
    }
    protected override void EnsureInvariants() => Guard.Against(Id.Value is null, "SystemMetric must have an Id.");
}
