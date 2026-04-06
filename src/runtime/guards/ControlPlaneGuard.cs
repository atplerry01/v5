using Whyce.Runtime.EventFabric;

namespace Whyce.Runtime.Guards;

/// <summary>
/// Control Plane Guard — validates that the runtime control plane
/// is properly configured with all mandatory components.
/// </summary>
public static class ControlPlaneGuard
{
    public static IReadOnlyList<string> Validate(
        Middleware.IMiddleware[]? middlewares,
        IEventFabric? eventFabric,
        Shared.Contracts.Runtime.ICommandDispatcher? dispatcher)
    {
        var violations = new List<string>();

        if (middlewares is null || middlewares.Length == 0)
            violations.Add("S0-CRITICAL: Middleware pipeline is empty.");

        if (eventFabric is null)
            violations.Add("S0-CRITICAL: EventFabric is not configured.");

        if (dispatcher is null)
            violations.Add("S0-CRITICAL: CommandDispatcher is not registered.");

        return violations;
    }
}
