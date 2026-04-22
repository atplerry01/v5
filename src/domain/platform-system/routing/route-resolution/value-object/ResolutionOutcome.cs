namespace Whycespace.Domain.PlatformSystem.Routing.RouteResolution;

public readonly record struct ResolutionOutcome
{
    public static readonly ResolutionOutcome Resolved = new("Resolved");
    public static readonly ResolutionOutcome Failed = new("Failed");
    public static readonly ResolutionOutcome Ambiguous = new("Ambiguous");

    public string Value { get; }

    private ResolutionOutcome(string value) => Value = value;
}
