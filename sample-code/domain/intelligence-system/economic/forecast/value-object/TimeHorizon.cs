namespace Whycespace.Domain.IntelligenceSystem.Economic.Forecast;

public sealed record TimeHorizon(string Value)
{
    public static readonly TimeHorizon Short = new("short");
    public static readonly TimeHorizon Medium = new("medium");
    public static readonly TimeHorizon Long = new("long");
}
