using Whycespace.Domain.PlatformSystem.Command.CommandMetadata;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Platform.Command.CommandMetadata;

namespace Whycespace.Engines.T2E.Platform.Command.CommandMetadata;

public sealed class AttachCommandMetadataHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not AttachCommandMetadataCommand cmd)
            return Task.CompletedTask;

        var aggregate = CommandMetadataAggregate.Attach(
            new CommandMetadataId(cmd.CommandMetadataId),
            cmd.EnvelopeRef,
            new MetadataActorId(cmd.ActorId),
            new MetadataTraceId(cmd.TraceId),
            new MetadataSpanId(cmd.SpanId),
            new PolicyContextRef(cmd.PolicyId, cmd.PolicyVersion),
            new TrustScore(cmd.TrustScore),
            new Timestamp(cmd.IssuedAt));

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
