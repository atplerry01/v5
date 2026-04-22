using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Routing.RouteDescriptor;

public readonly record struct RouteDescriptorId
{
    public Guid Value { get; }

    public RouteDescriptorId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "RouteDescriptorId cannot be empty.");
        Value = value;
    }
}
