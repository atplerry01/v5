using Whycespace.Domain.ControlSystem.Audit.AuditEvent;
using Whycespace.Shared.Contracts.Control.Audit.AuditEvent;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Control.Audit.AuditEvent;

public sealed class CaptureAuditEventHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CaptureAuditEventCommand cmd)
            return Task.CompletedTask;

        var aggregate = AuditEventAggregate.Capture(
            new AuditEventId(cmd.AuditEventId.ToString("N").PadRight(64, '0')),
            cmd.ActorId,
            cmd.Action,
            Enum.Parse<AuditEventKind>(cmd.Kind, ignoreCase: true),
            cmd.CorrelationId,
            cmd.OccurredAt);

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
