using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Envelope.Payload;

public sealed class PayloadSchemaAggregate : AggregateRoot
{
    public PayloadSchemaId PayloadSchemaId { get; private set; }
    public string TypeRef { get; private set; } = string.Empty;
    public PayloadEncoding Encoding { get; private set; }
    public string? SchemaRef { get; private set; }
    public int SchemaContractVersion { get; private set; }
    public long? MaxSizeBytes { get; private set; }
    public PayloadSchemaStatus Status { get; private set; }

    private PayloadSchemaAggregate() { }

    public static PayloadSchemaAggregate Register(
        PayloadSchemaId id,
        string typeRef,
        PayloadEncoding encoding,
        string? schemaRef,
        int schemaContractVersion,
        long? maxSizeBytes,
        Timestamp registeredAt)
    {
        var aggregate = new PayloadSchemaAggregate();
        if (aggregate.Version >= 0)
            throw PayloadSchemaErrors.AlreadyInitialized();

        if (string.IsNullOrWhiteSpace(typeRef))
            throw PayloadSchemaErrors.TypeRefMissing();

        if (encoding.RequiresSchemaRef && string.IsNullOrWhiteSpace(schemaRef))
            throw PayloadSchemaErrors.SchemaRefRequiredForEncoding();

        aggregate.RaiseDomainEvent(new PayloadSchemaRegisteredEvent(
            id, typeRef, encoding, schemaRef, schemaContractVersion, maxSizeBytes, registeredAt));

        return aggregate;
    }

    public void Deprecate(Timestamp deprecatedAt)
    {
        if (Status == PayloadSchemaStatus.Deprecated)
            throw PayloadSchemaErrors.AlreadyDeprecated();

        RaiseDomainEvent(new PayloadSchemaDeprecatedEvent(PayloadSchemaId, deprecatedAt));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case PayloadSchemaRegisteredEvent e:
                PayloadSchemaId = e.PayloadSchemaId;
                TypeRef = e.TypeRef;
                Encoding = e.Encoding;
                SchemaRef = e.SchemaRef;
                SchemaContractVersion = e.SchemaContractVersion;
                MaxSizeBytes = e.MaxSizeBytes;
                Status = PayloadSchemaStatus.Active;
                break;

            case PayloadSchemaDeprecatedEvent:
                Status = PayloadSchemaStatus.Deprecated;
                break;
        }
    }
}
