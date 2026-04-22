using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Schema.Versioning;

public static class VersioningRuleErrors
{
    public static DomainException AlreadyInitialized() =>
        new DomainInvariantViolationException("VersioningRule has already been initialized.");

    public static DomainException SchemaRefMissing() =>
        new DomainInvariantViolationException("VersioningRule requires a non-empty SchemaRef.");

    public static DomainException InvalidVersionOrder() =>
        new DomainInvariantViolationException("VersioningRule ToVersion must be strictly greater than FromVersion.");

    public static DomainException EmptyChangeSummary() =>
        new DomainInvariantViolationException("VersioningRule requires at least one change in ChangeSummary.");

    public static DomainException VerdictAlreadyIssued() =>
        new DomainInvariantViolationException("VersioningRule verdict has already been issued.");
}
