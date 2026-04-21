using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Offering.CommercialShape.Configuration;

public sealed class ConfigurationAggregate : AggregateRoot
{
    private readonly Dictionary<string, ConfigurationOption> _options = new();

    public ConfigurationId Id { get; private set; }
    public ConfigurationName Name { get; private set; }
    public ConfigurationStatus Status { get; private set; }
    public IReadOnlyDictionary<string, ConfigurationOption> Options => _options;

    public static ConfigurationAggregate Create(ConfigurationId id, ConfigurationName name)
    {
        var aggregate = new ConfigurationAggregate();
        if (aggregate.Version >= 0)
            throw ConfigurationErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new ConfigurationCreatedEvent(id, name));
        return aggregate;
    }

    public void SetOption(ConfigurationOption option)
    {
        EnsureMutable();

        RaiseDomainEvent(new ConfigurationOptionSetEvent(Id, option));
    }

    public void RemoveOption(string key)
    {
        EnsureMutable();

        if (!_options.ContainsKey(key))
            throw ConfigurationErrors.OptionNotPresent(key);

        RaiseDomainEvent(new ConfigurationOptionRemovedEvent(Id, key));
    }

    public void Activate()
    {
        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ConfigurationErrors.InvalidStateTransition(Status, nameof(Activate));

        RaiseDomainEvent(new ConfigurationActivatedEvent(Id));
    }

    public void Archive()
    {
        if (Status == ConfigurationStatus.Archived)
            throw ConfigurationErrors.InvalidStateTransition(Status, nameof(Archive));

        RaiseDomainEvent(new ConfigurationArchivedEvent(Id));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case ConfigurationCreatedEvent e:
                Id = e.ConfigurationId;
                Name = e.Name;
                Status = ConfigurationStatus.Draft;
                break;
            case ConfigurationOptionSetEvent e:
                _options[e.Option.Key] = e.Option;
                break;
            case ConfigurationOptionRemovedEvent e:
                _options.Remove(e.Key);
                break;
            case ConfigurationActivatedEvent:
                Status = ConfigurationStatus.Active;
                break;
            case ConfigurationArchivedEvent:
                Status = ConfigurationStatus.Archived;
                break;
        }
    }

    private void EnsureMutable()
    {
        var specification = new CanMutateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ConfigurationErrors.ArchivedImmutable(Id);
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw ConfigurationErrors.MissingId();

        if (!Enum.IsDefined(Status))
            throw ConfigurationErrors.InvalidStateTransition(Status, "validate");
    }
}
