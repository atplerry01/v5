using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.TrustSystem.Identity.Consent;

public readonly record struct ConsentDescriptor
{
    public Guid IdentityReference { get; }
    public string ConsentScope { get; }
    public string ConsentPurpose { get; }

    public ConsentDescriptor(Guid identityReference, string consentScope, string consentPurpose)
    {
        Guard.Against(identityReference == Guid.Empty, "ConsentDescriptor.IdentityReference must not be empty.");
        Guard.Against(string.IsNullOrWhiteSpace(consentScope), "ConsentDescriptor.ConsentScope must not be empty.");
        Guard.Against(string.IsNullOrWhiteSpace(consentPurpose), "ConsentDescriptor.ConsentPurpose must not be empty.");

        IdentityReference = identityReference;
        ConsentScope = consentScope.Trim();
        ConsentPurpose = consentPurpose.Trim();
    }
}
