using Whycespace.Domain.PlatformSystem.Envelope.Metadata;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Platform.Envelope.Metadata;

namespace Whycespace.Engines.T2E.Platform.Envelope.Metadata;

public sealed class RegisterMessageMetadataSchemaHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RegisterMessageMetadataSchemaCommand cmd)
            return Task.CompletedTask;

        var aggregate = MessageMetadataSchemaAggregate.Register(
            new MetadataSchemaId(cmd.MetadataSchemaId),
            cmd.SchemaVersion,
            cmd.RequiredFields,
            cmd.OptionalFields,
            new Timestamp(cmd.RegisteredAt));

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
