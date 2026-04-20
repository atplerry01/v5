using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.Lifecycle.Version;

public readonly record struct VersionNumber : IComparable<VersionNumber>
{
    public int Major { get; }
    public int Minor { get; }

    public VersionNumber(int major, int minor)
    {
        Guard.Against(major < 1, "VersionNumber major must be >= 1.");
        Guard.Against(minor < 0, "VersionNumber minor must be >= 0.");
        Major = major;
        Minor = minor;
    }

    public int CompareTo(VersionNumber other)
    {
        var majorCmp = Major.CompareTo(other.Major);
        return majorCmp != 0 ? majorCmp : Minor.CompareTo(other.Minor);
    }

    public override string ToString() => $"{Major}.{Minor}";
}
