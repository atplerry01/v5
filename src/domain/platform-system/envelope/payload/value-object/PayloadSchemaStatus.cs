namespace Whycespace.Domain.PlatformSystem.Envelope.Payload;

public readonly record struct PayloadSchemaStatus
{
    public static readonly PayloadSchemaStatus Active = new("Active");
    public static readonly PayloadSchemaStatus Deprecated = new("Deprecated");

    public string Value { get; }

    private PayloadSchemaStatus(string value) => Value = value;
}
