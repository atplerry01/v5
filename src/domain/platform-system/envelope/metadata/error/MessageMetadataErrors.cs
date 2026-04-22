using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Envelope.Metadata;

public static class MessageMetadataErrors
{
    public static DomainException AlreadyInitialized() =>
        new DomainInvariantViolationException("MessageMetadataSchema has already been initialized.");

    public static DomainException RequiredFieldsMissing() =>
        new DomainInvariantViolationException("MessageMetadataSchema RequiredFields must include: CorrelationId, CausationId, IssuedAt, MessageVersion.");
}
