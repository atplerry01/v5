using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Schema.Serialization;

public static class SerializationFormatErrors
{
    public static DomainException AlreadyInitialized() =>
        new DomainInvariantViolationException("SerializationFormat has already been initialized.");

    public static DomainException FormatNameMissing() =>
        new DomainInvariantViolationException("SerializationFormat requires a non-empty FormatName.");

    public static DomainException SchemaRefRequiredForEncoding() =>
        new DomainInvariantViolationException("SerializationFormat: Avro and Protobuf encodings must carry a non-null SchemaRef.");

    public static DomainException LossyFieldsMustBeDocumented() =>
        new DomainInvariantViolationException("SerializationFormat: LossyWithDocumentedFields requires at least one 'lossyField' option per lossy field.");

    public static DomainException AlreadyDeprecated() =>
        new DomainInvariantViolationException("SerializationFormat is already deprecated.");
}
