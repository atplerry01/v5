namespace Whycespace.Domain.BusinessSystem.Logistic;

public sealed record Location(double Latitude, double Longitude, string Label = "")
{
    public static Location Unknown => new(0, 0, "unknown");
}
