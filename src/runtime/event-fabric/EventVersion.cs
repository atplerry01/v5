namespace Whyce.Runtime.EventFabric;

/// <summary>
/// Event version. Semantic versioning for event schemas.
/// All events MUST carry a version for backward-compatible evolution.
/// </summary>
public sealed record EventVersion
{
    public required int Major { get; init; }
    public required int Minor { get; init; }
    public required int Patch { get; init; }

    public static EventVersion Default => new() { Major = 1, Minor = 0, Patch = 0 };

    public override string ToString() => $"{Major}.{Minor}.{Patch}";

    public static EventVersion Parse(string version)
    {
        var parts = version.Split('.');
        return new EventVersion
        {
            Major = parts.Length > 0 ? int.Parse(parts[0]) : 1,
            Minor = parts.Length > 1 ? int.Parse(parts[1]) : 0,
            Patch = parts.Length > 2 ? int.Parse(parts[2]) : 0
        };
    }
}
