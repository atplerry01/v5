using Whycespace.Domain.ControlSystem.Scheduling.SystemJob;
using Whycespace.Shared.Contracts.Control.Scheduling.SystemJob;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Control.Scheduling.SystemJob;

public sealed class DefineSystemJobHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not DefineSystemJobCommand cmd)
            return Task.CompletedTask;

        var aggregate = SystemJobAggregate.Define(
            new SystemJobId(cmd.JobId.ToString("N").PadRight(64, '0')),
            cmd.Name,
            Enum.Parse<JobCategory>(cmd.Category, ignoreCase: true),
            cmd.Timeout);

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
