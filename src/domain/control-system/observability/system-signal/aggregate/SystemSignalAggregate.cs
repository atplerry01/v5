using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Observability.SystemSignal;

public sealed class SystemSignalAggregate : AggregateRoot
{
    public SystemSignalId Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public SignalKind Kind { get; private set; }
    public string Source { get; private set; } = string.Empty;
    public bool IsDeprecated { get; private set; }

    private SystemSignalAggregate() { }

    public static SystemSignalAggregate Define(
        SystemSignalId id,
        string name,
        SignalKind kind,
        string source)
    {
        Guard.Against(string.IsNullOrEmpty(name), SystemSignalErrors.SignalNameMustNotBeEmpty().Message);
        Guard.Against(string.IsNullOrEmpty(source), SystemSignalErrors.SourceMustNotBeEmpty().Message);

        var aggregate = new SystemSignalAggregate();
        aggregate.RaiseDomainEvent(new SystemSignalDefinedEvent(id, name, kind, source));
        return aggregate;
    }

    public void Deprecate()
    {
        Guard.Against(IsDeprecated, SystemSignalErrors.SignalAlreadyDeprecated().Message);

        RaiseDomainEvent(new SystemSignalDeprecatedEvent(Id));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case SystemSignalDefinedEvent e:
                Id = e.Id;
                Name = e.Name;
                Kind = e.Kind;
                Source = e.Source;
                break;
            case SystemSignalDeprecatedEvent:
                IsDeprecated = true;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        Guard.Against(Id.Value is null, "SystemSignal must have a non-empty Id.");
        Guard.Against(string.IsNullOrEmpty(Name), "SystemSignal must have a non-empty Name.");
        Guard.Against(string.IsNullOrEmpty(Source), "SystemSignal must have a non-empty Source.");
    }
}
