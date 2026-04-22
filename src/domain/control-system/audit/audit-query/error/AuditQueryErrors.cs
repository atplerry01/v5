using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Audit.AuditQuery;

public static class AuditQueryErrors
{
    public static DomainException QueryIdMustNotBeEmpty() =>
        new DomainInvariantViolationException("AuditQueryId must not be null or empty.");

    public static DomainException QueryIdMustBe64HexChars(string value) =>
        new DomainInvariantViolationException(
            $"AuditQueryId must be exactly 64 lowercase hex characters. Got: '{value}'.");

    public static DomainException IssuedByMustNotBeEmpty() =>
        new DomainInvariantViolationException("AuditQuery requires a non-empty issuedBy.");

    public static DomainException QueryTimeRangeToMustBeAfterFrom() =>
        new DomainInvariantViolationException("AuditQuery time range 'To' must be after 'From'.");

    public static DomainException QueryAlreadyCompleted() =>
        new DomainInvariantViolationException("AuditQuery is already completed.");

    public static DomainException QueryAlreadyExpired() =>
        new DomainInvariantViolationException("AuditQuery is already expired.");

    public static DomainException QueryMustBeIssuedBeforeCompletion() =>
        new DomainInvariantViolationException("AuditQuery must be in Issued status before it can be completed.");
}
