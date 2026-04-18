using Whycespace.Domain.EconomicSystem.Transaction.Transaction;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Transaction.Transaction;

using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Transaction.Transaction;

public sealed class InitiateTransactionHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not InitiateTransactionCommand cmd)
            return Task.CompletedTask;

        EnforcementGuard.RequireNotRestricted(context.EnforcementConstraint, context.IsSystem);

        var references = new List<TransactionReference>(cmd.References.Count);
        foreach (var r in cmd.References)
            references.Add(TransactionReference.Of(r.Kind, r.Id));

        var aggregate = TransactionAggregate.Initiate(
            TransactionId.From(cmd.TransactionId),
            cmd.Kind,
            references,
            new Timestamp(cmd.InitiatedAt));

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
