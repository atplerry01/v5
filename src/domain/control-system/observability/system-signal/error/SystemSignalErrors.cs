using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Observability.SystemSignal;

public static class SystemSignalErrors
{
    public static DomainException SignalIdMustNotBeEmpty() =>
        new DomainInvariantViolationException("SystemSignalId must not be null or empty.");

    public static DomainException SignalIdMustBe64HexChars(string value) =>
        new DomainInvariantViolationException(
            $"SystemSignalId must be exactly 64 lowercase hex characters. Got: '{value}'.");

    public static DomainException SignalNameMustNotBeEmpty() =>
        new DomainInvariantViolationException("SystemSignal name must not be null or empty.");

    public static DomainException SourceMustNotBeEmpty() =>
        new DomainInvariantViolationException("SystemSignal source must not be null or empty.");

    public static DomainException SignalAlreadyDeprecated() =>
        new DomainInvariantViolationException("SystemSignal is already deprecated.");
}
