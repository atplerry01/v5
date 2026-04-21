using Whycespace.Domain.BusinessSystem.Entitlement.UsageControl.UsageRight;
using Whycespace.Shared.Contracts.Business.Entitlement.UsageControl.UsageRight;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Entitlement.UsageControl.UsageRight;

public sealed class UseUsageRightHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not UseUsageRightCommand cmd)
            return;

        var aggregate = (UsageRightAggregate)await context.LoadAggregateAsync(typeof(UsageRightAggregate));
        // Domain entity reconstitution: Use(UsageRecord) takes a carrier entity; the
        // command flattens its fields so the engine rebuilds the value before calling
        // the aggregate. This preserves the domain signature without leaking engine
        // code into the domain.
        var record = new UsageRecord(new UsageRecordId(cmd.RecordId), cmd.UnitsUsed);
        aggregate.Use(record);
        context.EmitEvents(aggregate.DomainEvents);
    }
}
