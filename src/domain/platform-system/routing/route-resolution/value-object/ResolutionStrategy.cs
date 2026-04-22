namespace Whycespace.Domain.PlatformSystem.Routing.RouteResolution;

public readonly record struct ResolutionStrategy
{
    public static readonly ResolutionStrategy ExactMatch = new("ExactMatch");
    public static readonly ResolutionStrategy PrefixMatch = new("PrefixMatch");
    public static readonly ResolutionStrategy DefaultRoute = new("DefaultRoute");

    public string Value { get; }

    private ResolutionStrategy(string value) => Value = value;
}
