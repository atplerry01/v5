using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Revenue.Contract;

public sealed class ContractValidationService
{
    public bool IsWithinTerm(RevenueContractAggregate contract, Timestamp currentTime) =>
        contract.Status == ContractStatus.Active &&
        currentTime.Value >= contract.Term.StartDate.Value &&
        currentTime.Value <= contract.Term.EndDate.Value;

    public bool ValidateShareRules(RevenueContractAggregate contract)
    {
        if (contract.Parties.Count < 2) return false;

        var totalShare = 0m;
        foreach (var party in contract.Parties)
            totalShare += party.SharePercentage;

        return totalShare == 100m;
    }
}
