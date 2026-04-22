namespace Whycespace.Domain.PlatformSystem.Envelope.Payload;

public readonly record struct PayloadEncoding
{
    public static readonly PayloadEncoding Json = new("Json");
    public static readonly PayloadEncoding Avro = new("Avro");
    public static readonly PayloadEncoding Protobuf = new("Protobuf");
    public static readonly PayloadEncoding Binary = new("Binary");

    public string Value { get; }

    private PayloadEncoding(string value) => Value = value;

    public bool RequiresSchemaRef => Value is "Avro" or "Protobuf";
}
