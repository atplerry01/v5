using Whycespace.Domain.ControlSystem.Enforcement.Violation;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Enforcement.Violation;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Enforcement.Violation;

public sealed class ResolveViolationHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ResolveViolationCommand cmd)
            return;

        var aggregate = (ViolationAggregate)await context.LoadAggregateAsync(typeof(ViolationAggregate));
        aggregate.Resolve(cmd.Resolution, new Timestamp(cmd.ResolvedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
