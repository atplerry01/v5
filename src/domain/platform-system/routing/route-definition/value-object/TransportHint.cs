namespace Whycespace.Domain.PlatformSystem.Routing.RouteDefinition;

public readonly record struct TransportHint
{
    public static readonly TransportHint Kafka = new("Kafka");
    public static readonly TransportHint InProcess = new("InProcess");
    public static readonly TransportHint Http = new("Http");
    public static readonly TransportHint Grpc = new("Grpc");

    public string Value { get; }

    private TransportHint(string value) => Value = value;
}
