using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Envelope.Payload;

public sealed record PayloadValueObject
{
    public string TypeRef { get; }
    public PayloadEncoding Encoding { get; }
    public string? SchemaRef { get; }
    public ReadOnlyMemory<byte> Bytes { get; }

    public PayloadValueObject(
        string typeRef,
        PayloadEncoding encoding,
        string? schemaRef,
        ReadOnlyMemory<byte> bytes)
    {
        Guard.Against(string.IsNullOrWhiteSpace(typeRef), "PayloadValueObject requires a non-empty TypeRef.");
        Guard.Against(encoding.RequiresSchemaRef && string.IsNullOrWhiteSpace(schemaRef),
            "PayloadValueObject: Avro and Protobuf encodings must reference a SchemaRef.");
        Guard.Against(bytes.IsEmpty, "PayloadValueObject requires non-empty Bytes for dispatched messages.");

        TypeRef = typeRef;
        Encoding = encoding;
        SchemaRef = schemaRef;
        Bytes = bytes;
    }
}
