using Whycespace.Domain.PlatformSystem.Event.EventSchema;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Platform.Event.EventSchema;

namespace Whycespace.Engines.T2E.Platform.Event.EventSchema;

public sealed class RegisterEventSchemaHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RegisterEventSchemaCommand cmd)
            return Task.CompletedTask;

        var compatibilityMode = cmd.CompatibilityMode switch
        {
            "Backward" => CompatibilityMode.Backward,
            "Forward" => CompatibilityMode.Forward,
            "Full" => CompatibilityMode.Full,
            _ => CompatibilityMode.None
        };

        var aggregate = EventSchemaAggregate.Register(
            new EventSchemaId(cmd.EventSchemaId),
            new EventSchemaName(cmd.Name),
            new EventSchemaVersion(cmd.Version),
            compatibilityMode,
            new Timestamp(cmd.RegisteredAt));

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
