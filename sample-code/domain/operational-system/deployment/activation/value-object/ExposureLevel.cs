namespace Whycespace.Domain.OperationalSystem.Deployment.Activation;

/// <summary>
/// Controls the degree of system exposure for a region.
/// Canary: limited traffic. Full: all traffic. ReadOnly: monitoring only.
/// </summary>
public sealed record ExposureLevel
{
    public static readonly ExposureLevel ReadOnly = new("ReadOnly", 0);
    public static readonly ExposureLevel Canary = new("Canary", 10);
    public static readonly ExposureLevel Limited = new("Limited", 50);
    public static readonly ExposureLevel Full = new("Full", 100);

    public string Value { get; }
    public int TrafficPercent { get; }

    private ExposureLevel(string value, int trafficPercent)
    {
        Value = value;
        TrafficPercent = trafficPercent;
    }

    public static ExposureLevel Custom(int trafficPercent) =>
        trafficPercent < 0 || trafficPercent > 100
            ? throw new ArgumentOutOfRangeException(nameof(trafficPercent), "Must be 0-100.")
            : new("Custom", trafficPercent);
}
