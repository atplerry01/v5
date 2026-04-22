using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.TrustSystem.Identity.Trust;

public readonly record struct TrustDescriptor
{
    public Guid IdentityReference { get; }
    public string TrustCategory { get; }
    public decimal Score { get; }

    public TrustDescriptor(Guid identityReference, string trustCategory, decimal score)
    {
        Guard.Against(identityReference == Guid.Empty, "TrustDescriptor.IdentityReference must not be empty.");
        Guard.Against(string.IsNullOrWhiteSpace(trustCategory), "TrustDescriptor.TrustCategory must not be empty.");
        Guard.Against(score < 0m || score > 1m, "TrustDescriptor.Score must be between 0.0 and 1.0.");

        IdentityReference = identityReference;
        TrustCategory = trustCategory.Trim();
        Score = score;
    }
}
