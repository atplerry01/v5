using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Identifier.CausationId;

public static class CausationIdErrors
{
    public static DomainException ValueMustNotBeEmpty() =>
        new DomainInvariantViolationException("CausationId value must not be null or empty.");

    public static DomainException ValueMustBe64LowercaseHexChars(string value) =>
        new DomainInvariantViolationException(
            $"CausationId must be exactly 64 lowercase hex characters. Got: '{value}' (length={value.Length}).");
}
