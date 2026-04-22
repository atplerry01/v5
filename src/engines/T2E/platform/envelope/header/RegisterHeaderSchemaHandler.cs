using Whycespace.Domain.PlatformSystem.Envelope.Header;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Platform.Envelope.Header;

namespace Whycespace.Engines.T2E.Platform.Envelope.Header;

public sealed class RegisterHeaderSchemaHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RegisterHeaderSchemaCommand cmd)
            return Task.CompletedTask;

        var aggregate = HeaderSchemaAggregate.Register(
            new HeaderSchemaId(cmd.HeaderSchemaId),
            new HeaderKind(cmd.HeaderKind),
            cmd.SchemaVersion,
            cmd.RequiredFields,
            new Timestamp(cmd.RegisteredAt));

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
