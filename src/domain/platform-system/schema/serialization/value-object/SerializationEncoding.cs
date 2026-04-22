namespace Whycespace.Domain.PlatformSystem.Schema.Serialization;

public readonly record struct SerializationEncoding
{
    public static readonly SerializationEncoding Json = new("Json");
    public static readonly SerializationEncoding Avro = new("Avro");
    public static readonly SerializationEncoding Protobuf = new("Protobuf");
    public static readonly SerializationEncoding MsgPack = new("MsgPack");
    public static readonly SerializationEncoding Binary = new("Binary");

    public string Value { get; }

    private SerializationEncoding(string value) => Value = value;

    public bool RequiresSchemaRef => Value is "Avro" or "Protobuf";
}
