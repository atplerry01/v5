using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.TrustSystem.Identity.Federation;

public readonly record struct FederationDescriptor
{
    public Guid IdentityReference { get; }
    public string FederatedProvider { get; }
    public string FederationType { get; }

    public FederationDescriptor(Guid identityReference, string federatedProvider, string federationType)
    {
        Guard.Against(identityReference == Guid.Empty, "FederationDescriptor.IdentityReference must not be empty.");
        Guard.Against(string.IsNullOrWhiteSpace(federatedProvider), "FederationDescriptor.FederatedProvider must not be empty.");
        Guard.Against(string.IsNullOrWhiteSpace(federationType), "FederationDescriptor.FederationType must not be empty.");

        IdentityReference = identityReference;
        FederatedProvider = federatedProvider.Trim();
        FederationType = federationType.Trim();
    }
}
