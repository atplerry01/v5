using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Envelope.Payload;

public static class PayloadSchemaErrors
{
    public static DomainException AlreadyInitialized() =>
        new DomainInvariantViolationException("PayloadSchema has already been initialized.");

    public static DomainException TypeRefMissing() =>
        new DomainInvariantViolationException("PayloadSchema requires a non-empty TypeRef.");

    public static DomainException SchemaRefRequiredForEncoding() =>
        new DomainInvariantViolationException("PayloadSchema: Avro and Protobuf encodings must reference a SchemaRef.");

    public static DomainException AlreadyDeprecated() =>
        new DomainInvariantViolationException("PayloadSchema has already been deprecated.");
}
