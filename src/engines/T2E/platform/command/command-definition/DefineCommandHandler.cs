using Whycespace.Domain.PlatformSystem.Command.CommandDefinition;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Platform.Command.CommandDefinition;

namespace Whycespace.Engines.T2E.Platform.Command.CommandDefinition;

public sealed class DefineCommandHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not DefineCommandCommand cmd)
            return Task.CompletedTask;

        var aggregate = CommandDefinitionAggregate.Define(
            new CommandDefinitionId(cmd.CommandDefinitionId),
            new CommandTypeName(cmd.TypeName),
            new CommandVersion(cmd.Version),
            cmd.SchemaId,
            new DomainRoute(cmd.OwnerClassification, cmd.OwnerContext, cmd.OwnerDomain),
            new Timestamp(cmd.DefinedAt));

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
