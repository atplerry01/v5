using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.TrustSystem.Access.Request;

public readonly record struct RequestDescriptor
{
    public Guid PrincipalReference { get; }
    public string RequestType { get; }
    public string RequestScope { get; }

    public RequestDescriptor(Guid principalReference, string requestType, string requestScope)
    {
        Guard.Against(principalReference == Guid.Empty, "RequestDescriptor.PrincipalReference must not be empty.");
        Guard.Against(string.IsNullOrWhiteSpace(requestType), "RequestDescriptor.RequestType must not be empty.");
        Guard.Against(string.IsNullOrWhiteSpace(requestScope), "RequestDescriptor.RequestScope must not be empty.");

        PrincipalReference = principalReference;
        RequestType = requestType.Trim();
        RequestScope = requestScope.Trim();
    }
}
