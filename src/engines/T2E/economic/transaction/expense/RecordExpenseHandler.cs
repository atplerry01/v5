using Whycespace.Domain.EconomicSystem.Transaction.Expense;
using Whycespace.Shared.Contracts.Economic.Transaction.Expense;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Transaction.Expense;

public sealed class RecordExpenseHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RecordExpenseCommand cmd)
            return Task.CompletedTask;

        var aggregate = ExpenseAggregate.Create(
            new ExpenseId(cmd.ExpenseId),
            cmd.Amount,
            ExpenseMetadata.Of(cmd.Currency),
            ExpenseCategory.From(cmd.Category),
            ExpenseSourceReference.From(cmd.SourceReference));

        aggregate.Record();

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
