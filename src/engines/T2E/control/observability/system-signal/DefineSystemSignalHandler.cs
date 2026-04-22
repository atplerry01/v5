using Whycespace.Domain.ControlSystem.Observability.SystemSignal;
using Whycespace.Shared.Contracts.Control.Observability.SystemSignal;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Control.Observability.SystemSignal;

public sealed class DefineSystemSignalHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not DefineSystemSignalCommand cmd)
            return Task.CompletedTask;

        var aggregate = SystemSignalAggregate.Define(
            new SystemSignalId(cmd.SignalId.ToString("N").PadRight(64, '0')),
            cmd.Name,
            Enum.Parse<SignalKind>(cmd.Kind, ignoreCase: true),
            cmd.Source);

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
