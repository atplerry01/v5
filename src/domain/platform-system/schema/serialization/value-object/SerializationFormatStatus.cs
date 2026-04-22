namespace Whycespace.Domain.PlatformSystem.Schema.Serialization;

public readonly record struct SerializationFormatStatus
{
    public static readonly SerializationFormatStatus Active = new("Active");
    public static readonly SerializationFormatStatus Deprecated = new("Deprecated");

    public string Value { get; }

    private SerializationFormatStatus(string value) => Value = value;
}
