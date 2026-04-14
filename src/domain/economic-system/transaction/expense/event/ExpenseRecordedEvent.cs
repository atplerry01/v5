using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Transaction.Expense;

public sealed record ExpenseRecordedEvent(
    string ExpenseId,
    decimal Amount,
    string Currency) : DomainEvent;
