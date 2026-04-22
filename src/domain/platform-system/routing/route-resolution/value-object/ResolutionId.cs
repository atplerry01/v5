using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Routing.RouteResolution;

public readonly record struct ResolutionId
{
    public Guid Value { get; }

    public ResolutionId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "ResolutionId cannot be empty.");
        Value = value;
    }
}
