using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Routing.RouteDefinition;

public readonly record struct RouteDefinitionId
{
    public Guid Value { get; }

    public RouteDefinitionId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "RouteDefinitionId cannot be empty.");
        Value = value;
    }
}
