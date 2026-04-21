using Whycespace.Shared.Contracts.Business.Agreement.Commitment.Contract;
using Whycespace.Shared.Contracts.Events.Business.Agreement.Commitment.Contract;

namespace Whycespace.Projections.Business.Agreement.Commitment.Contract.Reducer;

public static class ContractProjectionReducer
{
    public static ContractReadModel Apply(ContractReadModel state, ContractCreatedEventSchema e) =>
        state with
        {
            ContractId = e.AggregateId,
            Status = "Draft",
            Parties = state.Parties,
            CreatedAt = e.CreatedAt,
            LastUpdatedAt = e.CreatedAt
        };

    public static ContractReadModel Apply(ContractReadModel state, ContractPartyAddedEventSchema e) =>
        state with
        {
            ContractId = e.AggregateId,
            Parties = state.Parties
                .Concat(new[] { new ContractPartyReadModel(e.PartyId, e.Role) })
                .ToList()
        };

    public static ContractReadModel Apply(ContractReadModel state, ContractActivatedEventSchema e) =>
        state with
        {
            ContractId = e.AggregateId,
            Status = "Active"
        };

    public static ContractReadModel Apply(ContractReadModel state, ContractSuspendedEventSchema e) =>
        state with
        {
            ContractId = e.AggregateId,
            Status = "Suspended"
        };

    public static ContractReadModel Apply(ContractReadModel state, ContractTerminatedEventSchema e) =>
        state with
        {
            ContractId = e.AggregateId,
            Status = "Terminated"
        };
}
