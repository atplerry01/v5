namespace Whycespace.Domain.ConstitutionalSystem.Policy.Registry;

using Whycespace.Domain.SharedKernel;

public sealed class FederationVersion : ValueObject
{
    public long VersionNumber { get; }
    public DateTimeOffset CreatedAt { get; }

    private FederationVersion(long versionNumber, DateTimeOffset createdAt)
    {
        VersionNumber = versionNumber;
        CreatedAt = createdAt;
    }

    public static FederationVersion Create(long versionNumber, DateTimeOffset timestamp)
    {
        if (versionNumber < 1) throw new ArgumentException("Version must be >= 1.", nameof(versionNumber));
        return new FederationVersion(versionNumber, timestamp);
    }

    public static FederationVersion From(long versionNumber, DateTimeOffset createdAt)
    {
        if (versionNumber < 1) throw new ArgumentException("Version must be >= 1.", nameof(versionNumber));
        return new FederationVersion(versionNumber, createdAt);
    }

    public bool IsNewerThan(FederationVersion other) => VersionNumber > other.VersionNumber;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return VersionNumber;
    }
}
