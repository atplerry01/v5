using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.LifecycleChange.Version;

public readonly record struct MediaVersionNumber : IComparable<MediaVersionNumber>
{
    public int Major { get; }
    public int Minor { get; }

    public MediaVersionNumber(int major, int minor)
    {
        Guard.Against(major < 1, "MediaVersionNumber major must be >= 1.");
        Guard.Against(minor < 0, "MediaVersionNumber minor must be >= 0.");
        Major = major;
        Minor = minor;
    }

    public int CompareTo(MediaVersionNumber other)
    {
        var majorCmp = Major.CompareTo(other.Major);
        return majorCmp != 0 ? majorCmp : Minor.CompareTo(other.Minor);
    }

    public override string ToString() => $"{Major}.{Minor}";
}
