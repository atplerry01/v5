using Whycespace.Domain.ControlSystem.Observability.SystemAlert;
using Whycespace.Shared.Contracts.Control.Observability.SystemAlert;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Control.Observability.SystemAlert;

public sealed class DefineSystemAlertHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not DefineSystemAlertCommand cmd)
            return Task.CompletedTask;

        var aggregate = SystemAlertAggregate.Define(
            new SystemAlertId(cmd.AlertId.ToString("N").PadRight(64, '0')),
            cmd.Name,
            cmd.MetricDefinitionId,
            cmd.ConditionExpression,
            Enum.Parse<AlertSeverity>(cmd.Severity, ignoreCase: true));

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
