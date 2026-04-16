using Whycespace.Domain.EconomicSystem.Revenue.Revenue;
using Whycespace.Shared.Contracts.Economic.Revenue.Revenue;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Infrastructure.Persistence;

namespace Whycespace.Engines.T2E.Economic.Revenue.Revenue;

public sealed class RecordRevenueHandler : IEngine
{
    private readonly IEventStore _eventStore;

    public RecordRevenueHandler(IEventStore eventStore)
    {
        _eventStore = eventStore;
    }

    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RecordRevenueCommand cmd)
            return;

        // DOM-LIFECYCLE-INIT-IDEMPOTENT-01.b (static-factory sub-clause):
        // load events directly from the store BEFORE invoking the factory.
        // The runtime's IEngineContext.LoadAggregateAsync throws DomainException
        // when the aggregate does not exist (AGGREGATE-EXISTENCE-GUARD), which
        // has the wrong polarity for a lifecycle-init check; we need to tolerate
        // "no prior events" and refuse "already exists". Direct event-store read
        // gives us that polarity without catching exceptions for control flow.
        var prior = await _eventStore.LoadEventsAsync(context.AggregateId);
        if (prior.Count > 0)
            throw RevenueErrors.AlreadyRecorded(RevenueId.From(cmd.RevenueId));

        var aggregate = RevenueAggregate.RecordRevenue(
            RevenueId.From(cmd.RevenueId),
            cmd.SpvId,
            cmd.Amount,
            cmd.Currency,
            cmd.SourceRef);

        context.EmitEvents(aggregate.DomainEvents);
    }
}
