namespace Whycespace.Domain.ContentSystem.Monetization.Payout;

public static class PayoutShareAllocator
{
    public static IReadOnlyDictionary<string, decimal> Allocate(decimal gross, IEnumerable<PayoutShare> shares)
    {
        if (gross <= 0m) throw PayoutErrors.InvalidGrossAmount();
        var list = shares?.ToList() ?? throw PayoutErrors.InvalidBeneficiary();
        if (list.Count == 0) throw PayoutErrors.SharesNotBalanced();
        var sum = list.Sum(s => s.Basis);
        if (sum != 1m) throw PayoutErrors.SharesNotBalanced();

        var allocations = new Dictionary<string, decimal>(StringComparer.Ordinal);
        decimal accumulated = 0m;
        for (var i = 0; i < list.Count; i++)
        {
            var share = list[i];
            decimal amount;
            if (i == list.Count - 1)
            {
                amount = gross - accumulated;
            }
            else
            {
                amount = decimal.Round(gross * share.Basis, 2, MidpointRounding.ToEven);
                accumulated += amount;
            }
            allocations[share.BeneficiaryRef] = allocations.TryGetValue(share.BeneficiaryRef, out var existing)
                ? existing + amount
                : amount;
        }
        return allocations;
    }
}
