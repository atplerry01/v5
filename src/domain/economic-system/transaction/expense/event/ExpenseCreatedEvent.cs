using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Transaction.Expense;

public sealed record ExpenseCreatedEvent(
    string ExpenseId,
    decimal Amount,
    string Currency,
    string Category,
    string SourceReference) : DomainEvent;
