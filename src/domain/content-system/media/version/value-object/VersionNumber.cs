using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Version;

public sealed record VersionNumber : ValueObject
{
    public int Major { get; }
    public int Minor { get; }
    public int Patch { get; }

    private VersionNumber(int major, int minor, int patch)
    {
        Major = major;
        Minor = minor;
        Patch = patch;
    }

    public static VersionNumber Create(int major, int minor, int patch)
    {
        if (major < 0 || minor < 0 || patch < 0)
            throw AssetVersionErrors.InvalidVersionNumber();
        return new VersionNumber(major, minor, patch);
    }

    public override string ToString() => $"{Major}.{Minor}.{Patch}";

    public bool IsGreaterThan(VersionNumber other)
    {
        if (Major != other.Major) return Major > other.Major;
        if (Minor != other.Minor) return Minor > other.Minor;
        return Patch > other.Patch;
    }
}
