namespace Whycespace.Domain.BusinessSystem.Document.Version;

public readonly record struct VersionNumber
{
    public int Major { get; }
    public int Minor { get; }

    public VersionNumber(int major, int minor)
    {
        if (major < 0)
            throw new ArgumentException("Major version must not be negative.", nameof(major));

        if (minor < 0)
            throw new ArgumentException("Minor version must not be negative.", nameof(minor));

        Major = major;
        Minor = minor;
    }

    public VersionNumber IncrementMinor() => new(Major, Minor + 1);

    public VersionNumber IncrementMajor() => new(Major + 1, 0);

    public override string ToString() => $"{Major}.{Minor}";
}
