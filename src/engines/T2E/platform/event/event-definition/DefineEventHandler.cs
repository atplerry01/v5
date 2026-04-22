using Whycespace.Domain.PlatformSystem.Event.EventDefinition;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Platform.Event.EventDefinition;

namespace Whycespace.Engines.T2E.Platform.Event.EventDefinition;

public sealed class DefineEventHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not DefineEventCommand cmd)
            return Task.CompletedTask;

        var aggregate = EventDefinitionAggregate.Define(
            new EventDefinitionId(cmd.EventDefinitionId),
            new EventTypeName(cmd.TypeName),
            new EventVersion(cmd.Version),
            cmd.SchemaId,
            new DomainRoute(cmd.SourceClassification, cmd.SourceContext, cmd.SourceDomain),
            new Timestamp(cmd.DefinedAt));

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
