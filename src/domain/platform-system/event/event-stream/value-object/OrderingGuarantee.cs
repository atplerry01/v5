namespace Whycespace.Domain.PlatformSystem.Event.EventStream;

public readonly record struct OrderingGuarantee
{
    public static readonly OrderingGuarantee Ordered = new("Ordered");
    public static readonly OrderingGuarantee Unordered = new("Unordered");

    public string Value { get; }

    private OrderingGuarantee(string value) => Value = value;
}
