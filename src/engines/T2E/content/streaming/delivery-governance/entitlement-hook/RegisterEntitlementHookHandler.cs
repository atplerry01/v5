using Whycespace.Domain.ContentSystem.Streaming.DeliveryGovernance.EntitlementHook;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Streaming.DeliveryGovernance.EntitlementHook;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Engines.T2E.Content.Streaming.DeliveryGovernance.EntitlementHook;

public sealed class RegisterEntitlementHookHandler : IEngine
{
    private readonly IClock _clock;

    public RegisterEntitlementHookHandler(IClock clock) => _clock = clock;

    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RegisterEntitlementHookCommand cmd) return Task.CompletedTask;
        var aggregate = EntitlementHookAggregate.Register(
            new EntitlementHookId(cmd.HookId),
            new EntitlementTargetRef(cmd.TargetId),
            new SourceSystemRef(cmd.SourceSystem),
            new Timestamp(_clock.UtcNow));
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
