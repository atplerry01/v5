namespace Whycespace.Domain.BusinessSystem.Document.Version;

public static class VersionErrors
{
    public static VersionDomainException MissingId()
        => new("VersionId is required and must not be empty.");

    public static VersionDomainException InvalidStateTransition(VersionStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static VersionDomainException MetadataRequired()
        => new("Version must have metadata assigned before release.");

    public static VersionDomainException ImmutableAfterSuperseded()
        => new("Cannot modify a superseded version.");

    public static VersionDomainException InvalidVersionNumber()
        => new("Version number must be valid for release.");
}
