using Whycespace.Domain.PlatformSystem.Schema.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Platform.Schema.Serialization;

namespace Whycespace.Engines.T2E.Platform.Schema.Serialization;

public sealed class RegisterSerializationFormatHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RegisterSerializationFormatCommand cmd)
            return Task.CompletedTask;

        var encoding = cmd.Encoding switch
        {
            "Avro" => SerializationEncoding.Avro,
            "Protobuf" => SerializationEncoding.Protobuf,
            "MsgPack" => SerializationEncoding.MsgPack,
            "Binary" => SerializationEncoding.Binary,
            _ => SerializationEncoding.Json
        };

        var roundTrip = cmd.RoundTripGuarantee == "LossyWithDocumentedFields"
            ? RoundTripGuarantee.LossyWithDocumentedFields
            : RoundTripGuarantee.Lossless;

        var options = cmd.Options
            .Select(o => new SerializationOption(o.Key, o.Value))
            .ToList();

        var aggregate = SerializationFormatAggregate.Register(
            new SerializationFormatId(cmd.SerializationFormatId),
            cmd.FormatName,
            encoding,
            cmd.SchemaRef,
            options,
            roundTrip,
            cmd.FormatVersion,
            new Timestamp(cmd.RegisteredAt));

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
