namespace Whycespace.Domain.BusinessSystem.Logistic.Route;

public readonly record struct Waypoint
{
    public string Value { get; }

    public Waypoint(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Waypoint must not be empty.", nameof(value));

        Value = value;
    }
}
