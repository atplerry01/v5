namespace Whycespace.Domain.PlatformSystem.Envelope.MessageEnvelope;

public readonly record struct EnvelopeStatus
{
    public static readonly EnvelopeStatus Created = new("Created");
    public static readonly EnvelopeStatus Dispatched = new("Dispatched");
    public static readonly EnvelopeStatus Acknowledged = new("Acknowledged");
    public static readonly EnvelopeStatus Rejected = new("Rejected");

    public string Value { get; }

    private EnvelopeStatus(string value) => Value = value;

    public bool IsTerminal => Value is "Acknowledged" or "Rejected";
}
