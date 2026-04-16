using Whycespace.Shared.Contracts.Economic.Revenue.Contract;
using Whycespace.Shared.Contracts.Events.Economic.Revenue.Contract;

namespace Whycespace.Projections.Economic.Revenue.Contract.Reducer;

public static class ContractProjectionReducer
{
    public static ContractReadModel Apply(ContractReadModel state, RevenueContractCreatedEventSchema e) =>
        state with
        {
            ContractId = e.AggregateId,
            Status = "Draft",
            TermStart = e.TermStart,
            TermEnd = e.TermEnd,
            PartyCount = e.ShareRules.Count
        };

    public static ContractReadModel Apply(ContractReadModel state, RevenueContractActivatedEventSchema _) =>
        state with { Status = "Active" };

    public static ContractReadModel Apply(ContractReadModel state, RevenueContractTerminatedEventSchema e) =>
        state with
        {
            Status = "Terminated",
            TerminationReason = e.Reason,
            TerminatedAt = e.TerminatedAt
        };
}
