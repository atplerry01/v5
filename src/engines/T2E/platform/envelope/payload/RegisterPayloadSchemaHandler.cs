using Whycespace.Domain.PlatformSystem.Envelope.Payload;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Platform.Envelope.Payload;

namespace Whycespace.Engines.T2E.Platform.Envelope.Payload;

public sealed class RegisterPayloadSchemaHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RegisterPayloadSchemaCommand cmd)
            return Task.CompletedTask;

        var encoding = cmd.Encoding switch
        {
            "Avro" => PayloadEncoding.Avro,
            "Protobuf" => PayloadEncoding.Protobuf,
            "Binary" => PayloadEncoding.Binary,
            _ => PayloadEncoding.Json
        };

        var aggregate = PayloadSchemaAggregate.Register(
            new PayloadSchemaId(cmd.PayloadSchemaId),
            cmd.TypeRef,
            encoding,
            cmd.SchemaRef,
            cmd.SchemaContractVersion,
            cmd.MaxSizeBytes,
            new Timestamp(cmd.RegisteredAt));

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
