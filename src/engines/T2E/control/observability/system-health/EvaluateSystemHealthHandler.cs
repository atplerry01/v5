using Whycespace.Domain.ControlSystem.Observability.SystemHealth;
using Whycespace.Shared.Contracts.Control.Observability.SystemHealth;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Control.Observability.SystemHealth;

public sealed class EvaluateSystemHealthHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not EvaluateSystemHealthCommand cmd)
            return Task.CompletedTask;

        var aggregate = SystemHealthAggregate.Evaluate(
            new SystemHealthId(cmd.HealthId.ToString("N").PadRight(64, '0')),
            cmd.ComponentName,
            Enum.Parse<HealthStatus>(cmd.Status, ignoreCase: true),
            cmd.EvaluatedAt);

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
