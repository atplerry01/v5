using Whycespace.Domain.ControlSystem.Observability.SystemTrace;
using Whycespace.Shared.Contracts.Control.Observability.SystemTrace;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Control.Observability.SystemTrace;

public sealed class StartSystemTraceHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not StartSystemTraceCommand cmd)
            return Task.CompletedTask;

        var aggregate = SystemTraceAggregate.Start(
            new SystemTraceId(cmd.SpanId.ToString("N").PadRight(64, '0')),
            cmd.TraceId,
            cmd.OperationName,
            cmd.StartedAt,
            cmd.ParentSpanId);

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
