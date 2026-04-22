using Whycespace.Domain.ControlSystem.SystemReconciliation.SystemVerification;
using Whycespace.Shared.Contracts.Control.SystemReconciliation.SystemVerification;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Control.SystemReconciliation.SystemVerification;

public sealed class PassSystemVerificationHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not PassSystemVerificationCommand cmd)
            return;

        var aggregate = (SystemVerificationAggregate)await context.LoadAggregateAsync(typeof(SystemVerificationAggregate));
        aggregate.Pass(cmd.PassedAt);
        context.EmitEvents(aggregate.DomainEvents);
    }
}
