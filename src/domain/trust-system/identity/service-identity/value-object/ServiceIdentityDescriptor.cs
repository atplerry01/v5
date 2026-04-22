using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.TrustSystem.Identity.ServiceIdentity;

public readonly record struct ServiceIdentityDescriptor
{
    public Guid OwnerReference { get; }
    public string ServiceName { get; }
    public string ServiceType { get; }

    public ServiceIdentityDescriptor(Guid ownerReference, string serviceName, string serviceType)
    {
        Guard.Against(ownerReference == Guid.Empty, "ServiceIdentityDescriptor.OwnerReference must not be empty.");
        Guard.Against(string.IsNullOrWhiteSpace(serviceName), "ServiceIdentityDescriptor.ServiceName must not be empty.");
        Guard.Against(string.IsNullOrWhiteSpace(serviceType), "ServiceIdentityDescriptor.ServiceType must not be empty.");

        OwnerReference = ownerReference;
        ServiceName = serviceName.Trim();
        ServiceType = serviceType.Trim();
    }
}
