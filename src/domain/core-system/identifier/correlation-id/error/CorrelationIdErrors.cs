using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Identifier.CorrelationId;

public static class CorrelationIdErrors
{
    public static DomainException ValueMustNotBeEmpty() =>
        new DomainInvariantViolationException("CorrelationId value must not be null or empty.");

    public static DomainException ValueMustBe64LowercaseHexChars(string value) =>
        new DomainInvariantViolationException(
            $"CorrelationId must be exactly 64 lowercase hex characters. Got: '{value}' (length={value.Length}).");
}
