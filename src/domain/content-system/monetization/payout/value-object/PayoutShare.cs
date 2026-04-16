using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Monetization.Payout;

public sealed record PayoutShare : ValueObject
{
    public string BeneficiaryRef { get; }
    public decimal Basis { get; }

    private PayoutShare(string beneficiaryRef, decimal basis)
    {
        BeneficiaryRef = beneficiaryRef;
        Basis = basis;
    }

    public static PayoutShare Create(string beneficiaryRef, decimal basis)
    {
        if (string.IsNullOrWhiteSpace(beneficiaryRef)) throw PayoutErrors.InvalidBeneficiary();
        if (basis <= 0m || basis > 1m) throw PayoutErrors.InvalidShareBasis();
        return new PayoutShare(beneficiaryRef.Trim(), basis);
    }
}
