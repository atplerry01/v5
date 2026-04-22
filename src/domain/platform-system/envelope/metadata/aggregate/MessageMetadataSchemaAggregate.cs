using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Envelope.Metadata;

public sealed class MessageMetadataSchemaAggregate : AggregateRoot
{
    private static readonly string[] MandatoryFields = ["CorrelationId", "CausationId", "IssuedAt", "MessageVersion"];

    public MetadataSchemaId MetadataSchemaId { get; private set; }
    public int SchemaVersion { get; private set; }
    public IReadOnlyList<string> RequiredFields { get; private set; } = [];
    public IReadOnlyList<string> OptionalFields { get; private set; } = [];

    private MessageMetadataSchemaAggregate() { }

    public static MessageMetadataSchemaAggregate Register(
        MetadataSchemaId id,
        int schemaVersion,
        IReadOnlyList<string> requiredFields,
        IReadOnlyList<string> optionalFields,
        Timestamp registeredAt)
    {
        var aggregate = new MessageMetadataSchemaAggregate();
        if (aggregate.Version >= 0)
            throw MessageMetadataErrors.AlreadyInitialized();

        if (!MandatoryFields.All(f => requiredFields.Contains(f)))
            throw MessageMetadataErrors.RequiredFieldsMissing();

        aggregate.RaiseDomainEvent(new MessageMetadataSchemaRegisteredEvent(
            id, schemaVersion, requiredFields, optionalFields, registeredAt));

        return aggregate;
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case MessageMetadataSchemaRegisteredEvent e:
                MetadataSchemaId = e.MetadataSchemaId;
                SchemaVersion = e.SchemaVersion;
                RequiredFields = e.RequiredFields;
                OptionalFields = e.OptionalFields;
                break;
        }
    }
}
