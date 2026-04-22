using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Envelope.Header;

public sealed class HeaderSchemaAggregate : AggregateRoot
{
    private static readonly string[] MandatoryFields = ["MessageId", "ContentType", "SourceAddress"];

    public HeaderSchemaId HeaderSchemaId { get; private set; }
    public HeaderKind HeaderKind { get; private set; }
    public int SchemaVersion { get; private set; }
    public IReadOnlyList<string> RequiredFields { get; private set; } = [];
    public HeaderSchemaStatus Status { get; private set; }

    private HeaderSchemaAggregate() { }

    public static HeaderSchemaAggregate Register(
        HeaderSchemaId id,
        HeaderKind headerKind,
        int schemaVersion,
        IReadOnlyList<string> requiredFields,
        Timestamp registeredAt)
    {
        var aggregate = new HeaderSchemaAggregate();
        if (aggregate.Version >= 0)
            throw HeaderSchemaErrors.AlreadyInitialized();

        if (!MandatoryFields.All(f => requiredFields.Contains(f)))
            throw HeaderSchemaErrors.RequiredFieldsMissing();

        aggregate.RaiseDomainEvent(new HeaderSchemaRegisteredEvent(
            id, headerKind, schemaVersion, requiredFields, registeredAt));

        return aggregate;
    }

    public void Deprecate(Timestamp deprecatedAt)
    {
        if (Status == HeaderSchemaStatus.Deprecated)
            throw HeaderSchemaErrors.AlreadyDeprecated();

        RaiseDomainEvent(new HeaderSchemaDeprecatedEvent(HeaderSchemaId, deprecatedAt));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case HeaderSchemaRegisteredEvent e:
                HeaderSchemaId = e.HeaderSchemaId;
                HeaderKind = e.HeaderKind;
                SchemaVersion = e.SchemaVersion;
                RequiredFields = e.RequiredFields;
                Status = HeaderSchemaStatus.Active;
                break;

            case HeaderSchemaDeprecatedEvent:
                Status = HeaderSchemaStatus.Deprecated;
                break;
        }
    }
}
