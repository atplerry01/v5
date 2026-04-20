namespace Whycespace.Domain.BusinessSystem.Offering.CommercialShape.Configuration;

public sealed class ConfigurationAggregate
{
    private readonly List<object> _uncommittedEvents = new();
    private readonly Dictionary<string, ConfigurationOption> _options = new();

    public ConfigurationId Id { get; private set; }
    public ConfigurationName Name { get; private set; }
    public ConfigurationStatus Status { get; private set; }
    public IReadOnlyDictionary<string, ConfigurationOption> Options => _options;
    public int Version { get; private set; }

    private ConfigurationAggregate() { }

    public static ConfigurationAggregate Create(ConfigurationId id, ConfigurationName name)
    {
        var aggregate = new ConfigurationAggregate();

        var @event = new ConfigurationCreatedEvent(id, name);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void SetOption(ConfigurationOption option)
    {
        EnsureMutable();

        var @event = new ConfigurationOptionSetEvent(Id, option);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void RemoveOption(string key)
    {
        EnsureMutable();

        if (!_options.ContainsKey(key))
            throw ConfigurationErrors.OptionNotPresent(key);

        var @event = new ConfigurationOptionRemovedEvent(Id, key);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Activate()
    {
        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ConfigurationErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new ConfigurationActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Archive()
    {
        if (Status == ConfigurationStatus.Archived)
            throw ConfigurationErrors.InvalidStateTransition(Status, nameof(Archive));

        var @event = new ConfigurationArchivedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(ConfigurationCreatedEvent @event)
    {
        Id = @event.ConfigurationId;
        Name = @event.Name;
        Status = ConfigurationStatus.Draft;
        Version++;
    }

    private void Apply(ConfigurationOptionSetEvent @event)
    {
        _options[@event.Option.Key] = @event.Option;
        Version++;
    }

    private void Apply(ConfigurationOptionRemovedEvent @event)
    {
        _options.Remove(@event.Key);
        Version++;
    }

    private void Apply(ConfigurationActivatedEvent @event)
    {
        Status = ConfigurationStatus.Active;
        Version++;
    }

    private void Apply(ConfigurationArchivedEvent @event)
    {
        Status = ConfigurationStatus.Archived;
        Version++;
    }

    private void EnsureMutable()
    {
        var specification = new CanMutateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ConfigurationErrors.ArchivedImmutable(Id);
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);

    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw ConfigurationErrors.MissingId();

        if (!Enum.IsDefined(Status))
            throw ConfigurationErrors.InvalidStateTransition(Status, "validate");
    }
}
