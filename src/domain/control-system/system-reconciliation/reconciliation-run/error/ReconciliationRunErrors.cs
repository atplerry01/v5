using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.SystemReconciliation.ReconciliationRun;

public static class ReconciliationRunErrors
{
    public static DomainException RunIdMustNotBeEmpty() =>
        new DomainInvariantViolationException("ReconciliationRunId must not be null or empty.");

    public static DomainException RunIdMustBe64HexChars(string value) =>
        new DomainInvariantViolationException(
            $"ReconciliationRunId must be exactly 64 lowercase hex characters. Got: '{value}'.");

    public static DomainException ScopeMustNotBeEmpty() =>
        new DomainInvariantViolationException("ReconciliationRun scope must not be null or empty.");

    public static DomainException RunNotPending() =>
        new DomainInvariantViolationException("ReconciliationRun must be in Pending status before it can be started.");

    public static DomainException RunNotRunning() =>
        new DomainInvariantViolationException("ReconciliationRun must be in Running status before it can be completed or aborted.");

    public static DomainException AbortReasonMustNotBeEmpty() =>
        new DomainInvariantViolationException("An abort reason must be provided when aborting a reconciliation run.");

    public static DomainException RunAlreadyTerminated(RunStatus status) =>
        new DomainInvariantViolationException($"ReconciliationRun is already in '{status}' status and cannot be modified.");
}
