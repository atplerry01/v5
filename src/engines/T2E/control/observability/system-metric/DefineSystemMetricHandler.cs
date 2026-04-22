using Whycespace.Domain.ControlSystem.Observability.SystemMetric;
using Whycespace.Shared.Contracts.Control.Observability.SystemMetric;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Control.Observability.SystemMetric;

public sealed class DefineSystemMetricHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not DefineSystemMetricCommand cmd)
            return Task.CompletedTask;

        var aggregate = SystemMetricAggregate.Define(
            new SystemMetricId(cmd.MetricId.ToString("N").PadRight(64, '0')),
            cmd.Name,
            Enum.Parse<MetricType>(cmd.Type, ignoreCase: true),
            cmd.Unit,
            cmd.LabelNames);

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
