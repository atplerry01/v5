namespace Whycespace.Domain.BusinessSystem.Document.Version;

public sealed class VersionMetadata
{
    public VersionNumber Number { get; }
    public Guid LineageId { get; }
    public Guid? ParentVersionId { get; }

    public VersionMetadata(VersionNumber number, Guid lineageId, Guid? parentVersionId)
    {
        if (lineageId == Guid.Empty)
            throw new ArgumentException("LineageId must not be empty.", nameof(lineageId));

        if (parentVersionId.HasValue && parentVersionId.Value == Guid.Empty)
            throw new ArgumentException("ParentVersionId must not be empty when provided.", nameof(parentVersionId));

        Number = number;
        LineageId = lineageId;
        ParentVersionId = parentVersionId;
    }
}
