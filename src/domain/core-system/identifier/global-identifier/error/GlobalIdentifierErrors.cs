using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Identifier.GlobalIdentifier;

public static class GlobalIdentifierErrors
{
    public static DomainException ValueMustNotBeEmpty() =>
        new DomainInvariantViolationException("GlobalIdentifier value must not be null or empty.");

    public static DomainException ValueMustBe64LowercaseHexChars(string value) =>
        new DomainInvariantViolationException(
            $"GlobalIdentifier must be exactly 64 lowercase hex characters. Got: '{value}' (length={value.Length}).");
}
