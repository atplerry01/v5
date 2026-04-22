using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Audit.AuditTrace;

public static class AuditTraceErrors
{
    public static DomainException TraceIdMustNotBeEmpty() =>
        new DomainInvariantViolationException("AuditTraceId must not be null or empty.");

    public static DomainException TraceIdMustBe64HexChars(string value) =>
        new DomainInvariantViolationException(
            $"AuditTraceId must be exactly 64 lowercase hex characters. Got: '{value}'.");

    public static DomainException CorrelationIdMustNotBeEmpty() =>
        new DomainInvariantViolationException("AuditTrace requires a non-empty correlationId.");

    public static DomainException AuditEventIdMustNotBeEmpty() =>
        new DomainInvariantViolationException("AuditTrace event link requires a non-empty auditEventId.");

    public static DomainException TraceAlreadyClosed() =>
        new DomainInvariantViolationException("AuditTrace is already closed.");

    public static DomainException CannotLinkEventToClosedTrace() =>
        new DomainInvariantViolationException("Cannot link an event to a closed AuditTrace.");
}
