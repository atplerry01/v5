using Whycespace.Shared.Contracts.Business.Agreement.PartyGovernance.Counterparty;
using Whycespace.Shared.Contracts.Events.Business.Agreement.PartyGovernance.Counterparty;

namespace Whycespace.Projections.Business.Agreement.PartyGovernance.Counterparty.Reducer;

public static class CounterpartyProjectionReducer
{
    public static CounterpartyReadModel Apply(CounterpartyReadModel state, CounterpartyCreatedEventSchema e) =>
        state with
        {
            CounterpartyId = e.AggregateId,
            Status = "Active",
            CreatedAt = DateTimeOffset.MinValue,
            LastUpdatedAt = DateTimeOffset.MinValue
        };

    public static CounterpartyReadModel Apply(CounterpartyReadModel state, CounterpartySuspendedEventSchema e) =>
        state with
        {
            CounterpartyId = e.AggregateId,
            Status = "Suspended"
        };

    public static CounterpartyReadModel Apply(CounterpartyReadModel state, CounterpartyTerminatedEventSchema e) =>
        state with
        {
            CounterpartyId = e.AggregateId,
            Status = "Terminated"
        };
}
