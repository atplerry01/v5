namespace Whycespace.Domain.PlatformSystem.Envelope.Header;

public readonly record struct HeaderSchemaStatus
{
    public static readonly HeaderSchemaStatus Active = new("Active");
    public static readonly HeaderSchemaStatus Deprecated = new("Deprecated");

    public string Value { get; }

    private HeaderSchemaStatus(string value) => Value = value;
}
