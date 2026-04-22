using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Audit.AuditRecord;

public static class AuditRecordErrors
{
    public static DomainException IdMustNotBeEmpty() =>
        new DomainInvariantViolationException("AuditRecordId must not be null or empty.");

    public static DomainException IdMustBe64HexChars(string value) =>
        new DomainInvariantViolationException(
            $"AuditRecordId must be exactly 64 lowercase hex characters. Got: '{value}'.");
}
