using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Identifier.EntityReference;

public static class EntityReferenceErrors
{
    public static DomainException IdentifierMustNotBeEmpty() =>
        new DomainInvariantViolationException("EntityReference identifier value must not be null or empty.");

    public static DomainException IdentifierMustBe64LowercaseHexChars(string value) =>
        new DomainInvariantViolationException(
            $"EntityReference identifier must be exactly 64 lowercase hex characters. Got: '{value}'.");

    public static DomainException EntityTypeMustNotBeEmpty() =>
        new DomainInvariantViolationException("EntityReference entityType must not be null or empty.");

    public static DomainException EntityTypeMustFollowThreeSegmentFormat(string entityType) =>
        new DomainInvariantViolationException(
            $"EntityReference entityType must follow '{"{classification}/{context}/{domain}"}' format. Got: '{entityType}'.");
}
