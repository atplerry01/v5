using Whycespace.Domain.ControlSystem.Audit.AuditTrace;
using Whycespace.Shared.Contracts.Control.Audit.AuditTrace;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Control.Audit.AuditTrace;

public sealed class OpenAuditTraceHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not OpenAuditTraceCommand cmd)
            return Task.CompletedTask;

        var aggregate = AuditTraceAggregate.Open(
            new AuditTraceId(cmd.TraceId.ToString("N").PadRight(64, '0')),
            cmd.CorrelationId,
            cmd.OpenedAt);

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
