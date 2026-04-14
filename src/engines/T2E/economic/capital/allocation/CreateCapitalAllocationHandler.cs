using Whycespace.Domain.EconomicSystem.Capital.Allocation;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Capital.Allocation;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Capital.Allocation;

public sealed class CreateCapitalAllocationHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateCapitalAllocationCommand cmd)
            return Task.CompletedTask;

        var aggregate = new CapitalAllocationAggregate();
        aggregate.Allocate(
            new AllocationId(cmd.AllocationId),
            cmd.SourceAccountId,
            new TargetId(cmd.TargetId),
            new Amount(cmd.Amount),
            new Currency(cmd.Currency),
            new Timestamp(cmd.AllocatedAt));

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
