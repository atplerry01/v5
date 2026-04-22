using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Schema.Contract;

public sealed class ContractAggregate : AggregateRoot
{
    public ContractId ContractId { get; private set; }
    public string ContractName { get; private set; } = string.Empty;
    public DomainRoute PublisherRoute { get; private set; } = null!;
    public Guid SchemaRef { get; private set; }
    public int SchemaVersion { get; private set; }
    public IReadOnlyList<SubscriberConstraint> SubscriberConstraints { get; private set; } = [];
    public ContractStatus Status { get; private set; }

    private ContractAggregate() { }

    public static ContractAggregate Register(
        ContractId id,
        string contractName,
        DomainRoute publisherRoute,
        Guid schemaRef,
        int schemaVersion,
        Timestamp registeredAt)
    {
        var aggregate = new ContractAggregate();
        if (aggregate.Version >= 0)
            throw ContractErrors.AlreadyInitialized();

        if (string.IsNullOrWhiteSpace(contractName))
            throw ContractErrors.ContractNameMissing();

        if (!publisherRoute.IsValid())
            throw ContractErrors.PublisherRouteMissing();

        if (schemaRef == Guid.Empty)
            throw ContractErrors.SchemaRefMissing();

        aggregate.RaiseDomainEvent(new ContractRegisteredEvent(
            id, contractName, publisherRoute, schemaRef, schemaVersion, registeredAt));

        return aggregate;
    }

    public void AddSubscriber(SubscriberConstraint constraint, Timestamp addedAt)
    {
        if (Status == ContractStatus.Deprecated)
            throw ContractErrors.AlreadyDeprecated();

        RaiseDomainEvent(new ContractSubscriberAddedEvent(ContractId, constraint, addedAt));
    }

    public void Deprecate(Timestamp deprecatedAt)
    {
        if (Status == ContractStatus.Deprecated)
            throw ContractErrors.AlreadyDeprecated();

        RaiseDomainEvent(new ContractDeprecatedEvent(ContractId, deprecatedAt));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case ContractRegisteredEvent e:
                ContractId = e.ContractId;
                ContractName = e.ContractName;
                PublisherRoute = e.PublisherRoute;
                SchemaRef = e.SchemaRef;
                SchemaVersion = e.SchemaVersion;
                SubscriberConstraints = [];
                Status = ContractStatus.Active;
                break;

            case ContractSubscriberAddedEvent e:
                SubscriberConstraints = [..SubscriberConstraints, e.Constraint];
                break;

            case ContractDeprecatedEvent:
                Status = ContractStatus.Deprecated;
                break;
        }
    }
}
