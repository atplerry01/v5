using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.TrustSystem.Access.Grant;

public readonly record struct GrantDescriptor
{
    public Guid PrincipalReference { get; }
    public string GrantScope { get; }
    public string GrantType { get; }

    public GrantDescriptor(Guid principalReference, string grantScope, string grantType)
    {
        Guard.Against(principalReference == Guid.Empty, "GrantDescriptor.PrincipalReference must not be empty.");
        Guard.Against(string.IsNullOrWhiteSpace(grantScope), "GrantDescriptor.GrantScope must not be empty.");
        Guard.Against(string.IsNullOrWhiteSpace(grantType), "GrantDescriptor.GrantType must not be empty.");

        PrincipalReference = principalReference;
        GrantScope = grantScope.Trim();
        GrantType = grantType.Trim();
    }
}
