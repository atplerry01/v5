using Whycespace.Domain.ControlSystem.Audit.AuditQuery;
using Whycespace.Shared.Contracts.Control.Audit.AuditQuery;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Control.Audit.AuditQuery;

public sealed class IssueAuditQueryHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not IssueAuditQueryCommand cmd)
            return Task.CompletedTask;

        var aggregate = AuditQueryAggregate.Issue(
            new AuditQueryId(cmd.QueryId.ToString("N").PadRight(64, '0')),
            cmd.IssuedBy,
            new QueryTimeRange(cmd.TimeRangeFrom, cmd.TimeRangeTo),
            cmd.CorrelationFilter,
            cmd.ActorFilter);

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
