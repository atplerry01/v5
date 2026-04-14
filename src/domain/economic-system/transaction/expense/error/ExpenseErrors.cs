using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Transaction.Expense;

public static class ExpenseErrors
{
    public static DomainException InvalidAmount() =>
        new("Expense amount must be greater than zero.");

    public static DomainException InvalidStateTransition(ExpenseStatus from, ExpenseStatus to) =>
        new($"Invalid expense state transition: {from} -> {to}.");

    public static DomainException AlreadyRecorded() =>
        new("Expense has already been recorded.");

    public static DomainException MissingSourceReference() =>
        new("Expense must reference a source document.");

    public static DomainInvariantViolationException NegativeAmount() =>
        new("Invariant violated: expense amount cannot be negative.");
}
