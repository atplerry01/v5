namespace Whycespace.Domain.PlatformSystem.Envelope.MessageEnvelope;

public readonly record struct MessageKind
{
    public static readonly MessageKind Command = new("Command");
    public static readonly MessageKind Event = new("Event");
    public static readonly MessageKind Notification = new("Notification");
    public static readonly MessageKind Query = new("Query");

    public string Value { get; }

    private MessageKind(string value) => Value = value;
}
