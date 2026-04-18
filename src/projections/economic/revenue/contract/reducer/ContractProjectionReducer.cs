using Whycespace.Shared.Contracts.Economic.Revenue.Contract;
using Whycespace.Shared.Contracts.Events.Economic.Revenue.Contract;

namespace Whycespace.Projections.Economic.Revenue.Contract.Reducer;

public static class ContractProjectionReducer
{
    public static ContractReadModel Apply(ContractReadModel state, RevenueContractCreatedEventSchema e)
    {
        var parties = new List<ContractPartyShare>(e.ShareRules.Count);
        foreach (var r in e.ShareRules)
            parties.Add(new ContractPartyShare(r.PartyId, r.SharePercentage));

        return state with
        {
            ContractId = e.AggregateId,
            Status = "Draft",
            TermStart = e.TermStart,
            TermEnd = e.TermEnd,
            PartyCount = e.ShareRules.Count,
            Parties = parties
        };
    }

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
