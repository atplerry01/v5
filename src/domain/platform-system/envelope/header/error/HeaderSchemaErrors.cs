using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Envelope.Header;

public static class HeaderSchemaErrors
{
    public static DomainException AlreadyInitialized() =>
        new DomainInvariantViolationException("HeaderSchema has already been initialized.");

    public static DomainException RequiredFieldsMissing() =>
        new DomainInvariantViolationException("HeaderSchema RequiredFields must include at minimum: MessageId, ContentType, SourceAddress.");

    public static DomainException AlreadyDeprecated() =>
        new DomainInvariantViolationException("HeaderSchema has already been deprecated.");
}
