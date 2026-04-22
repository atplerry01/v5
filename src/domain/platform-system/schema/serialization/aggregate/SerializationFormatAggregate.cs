using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Schema.Serialization;

public sealed class SerializationFormatAggregate : AggregateRoot
{
    public SerializationFormatId SerializationFormatId { get; private set; }
    public string FormatName { get; private set; } = string.Empty;
    public SerializationEncoding Encoding { get; private set; }
    public Guid? SchemaRef { get; private set; }
    public IReadOnlyList<SerializationOption> Options { get; private set; } = [];
    public RoundTripGuarantee RoundTripGuarantee { get; private set; }
    public int FormatVersion { get; private set; }
    public SerializationFormatStatus Status { get; private set; }

    private SerializationFormatAggregate() { }

    public static SerializationFormatAggregate Register(
        SerializationFormatId id,
        string formatName,
        SerializationEncoding encoding,
        Guid? schemaRef,
        IReadOnlyList<SerializationOption> options,
        RoundTripGuarantee roundTripGuarantee,
        int formatVersion,
        Timestamp registeredAt)
    {
        var aggregate = new SerializationFormatAggregate();
        if (aggregate.Version >= 0)
            throw SerializationFormatErrors.AlreadyInitialized();

        if (string.IsNullOrWhiteSpace(formatName))
            throw SerializationFormatErrors.FormatNameMissing();

        if (encoding.RequiresSchemaRef && (schemaRef is null || schemaRef == Guid.Empty))
            throw SerializationFormatErrors.SchemaRefRequiredForEncoding();

        if (roundTripGuarantee == RoundTripGuarantee.LossyWithDocumentedFields)
        {
            var hasLossyDocumentation = options?.Any(o => o.Key == "lossyField") ?? false;
            if (!hasLossyDocumentation)
                throw SerializationFormatErrors.LossyFieldsMustBeDocumented();
        }

        aggregate.RaiseDomainEvent(new SerializationFormatRegisteredEvent(
            id, formatName, encoding, schemaRef, options ?? [], roundTripGuarantee, formatVersion, registeredAt));

        return aggregate;
    }

    public void Deprecate(Timestamp deprecatedAt)
    {
        if (Status == SerializationFormatStatus.Deprecated)
            throw SerializationFormatErrors.AlreadyDeprecated();

        RaiseDomainEvent(new SerializationFormatDeprecatedEvent(SerializationFormatId, deprecatedAt));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case SerializationFormatRegisteredEvent e:
                SerializationFormatId = e.SerializationFormatId;
                FormatName = e.FormatName;
                Encoding = e.Encoding;
                SchemaRef = e.SchemaRef;
                Options = e.Options;
                RoundTripGuarantee = e.RoundTripGuarantee;
                FormatVersion = e.FormatVersion;
                Status = SerializationFormatStatus.Active;
                break;

            case SerializationFormatDeprecatedEvent:
                Status = SerializationFormatStatus.Deprecated;
                break;
        }
    }
}
