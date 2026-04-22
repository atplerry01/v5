using Whycespace.Domain.ControlSystem.Observability.SystemTrace;
using Whycespace.Shared.Contracts.Control.Observability.SystemTrace;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Control.Observability.SystemTrace;

public sealed class CompleteSystemTraceHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CompleteSystemTraceCommand cmd)
            return;

        var aggregate = (SystemTraceAggregate)await context.LoadAggregateAsync(typeof(SystemTraceAggregate));
        aggregate.Complete(cmd.CompletedAt, Enum.Parse<SpanStatus>(cmd.Status, ignoreCase: true));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
