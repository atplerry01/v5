using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Transaction.Expense;

public sealed record ExpenseCancelledEvent(
    string ExpenseId,
    string Reason) : DomainEvent;
