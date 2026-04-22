using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.TrustSystem.Access.Session;

public readonly record struct SessionDescriptor
{
    public Guid IdentityReference { get; }
    public string SessionContext { get; }

    public SessionDescriptor(Guid identityReference, string sessionContext)
    {
        Guard.Against(identityReference == Guid.Empty, "SessionDescriptor.IdentityReference must not be empty.");
        Guard.Against(string.IsNullOrWhiteSpace(sessionContext), "SessionDescriptor.SessionContext must not be empty.");

        IdentityReference = identityReference;
        SessionContext = sessionContext.Trim();
    }
}
