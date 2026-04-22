namespace Whycespace.Domain.PlatformSystem.Routing.DispatchRule;

public readonly record struct DispatchConditionType
{
    public static readonly DispatchConditionType MessageKindMatch = new("MessageKindMatch");
    public static readonly DispatchConditionType SourceClassificationMatch = new("SourceClassificationMatch");
    public static readonly DispatchConditionType DestinationContextMatch = new("DestinationContextMatch");
    public static readonly DispatchConditionType TransportHintMatch = new("TransportHintMatch");
    public static readonly DispatchConditionType AlwaysMatch = new("AlwaysMatch");

    public string Value { get; }

    private DispatchConditionType(string value) => Value = value;
}
