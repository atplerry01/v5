using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Audit.AuditEvent;

public static class AuditEventErrors
{
    public static DomainException AuditEventIdMustNotBeEmpty() =>
        new DomainInvariantViolationException("AuditEventId must not be null or empty.");

    public static DomainException AuditEventIdMustBe64HexChars(string value) =>
        new DomainInvariantViolationException(
            $"AuditEventId must be exactly 64 lowercase hex characters. Got: '{value}'.");

    public static DomainException ActorIdMustNotBeEmpty() =>
        new DomainInvariantViolationException("AuditEvent requires a non-empty actorId.");

    public static DomainException ActionMustNotBeEmpty() =>
        new DomainInvariantViolationException("AuditEvent requires a non-empty action.");

    public static DomainException CorrelationIdMustNotBeEmpty() =>
        new DomainInvariantViolationException("AuditEvent requires a non-empty correlationId.");

    public static DomainException AuditEventAlreadySealed() =>
        new DomainInvariantViolationException("AuditEvent is already sealed and cannot be modified.");
}
