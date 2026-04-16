using Whycespace.Domain.EconomicSystem.Transaction.Charge;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Transaction.Charge;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Transaction.Charge;

public sealed class ApplyChargeHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ApplyChargeCommand cmd)
            return;

        var aggregate = (ChargeAggregate)await context.LoadAggregateAsync(typeof(ChargeAggregate));

        aggregate.ApplyCharge(new Timestamp(cmd.AppliedAt));

        context.EmitEvents(aggregate.DomainEvents);
    }
}
