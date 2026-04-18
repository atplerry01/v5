using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Shared.ContentRef;

/// <summary>
/// Shared-kernel value object. The ONLY cross-domain reference permitted
/// inside content-system. Aggregates in delivery, interaction, engagement,
/// organization, access, discovery, monetization, moderation, and lifecycle
/// hold a ContentRef — never the authoring domain's own typed id.
///
/// Consuming domains resolve a ContentRef to denormalized content metadata
/// via an IContentRefResolver port implemented in the application /
/// infrastructure layer (out of scope for the domain layer; sourced from a
/// projection, not from the authoring aggregate directly).
///
/// ContentRef is immutable. A new ContentRef with Version+1 replaces an
/// older one when the authoring aggregate emits a revision event consuming
/// domains must observe.
/// </summary>
public readonly record struct ContentRef(ContentType Type, ContentId Id, ContentVersion Version)
{
    public static ContentRef Create(ContentType type, ContentId id, ContentVersion version)
    {
        Guard.Against(id.Value == Guid.Empty, "ContentRef requires a non-empty ContentId.");
        Guard.Against(version.Value < 1, "ContentRef version must be >= 1.");
        return new ContentRef(type, id, version);
    }

    /// <summary>
    /// URN form: <c>urn:content:{type}:{id}:{version}</c>. Deterministic for
    /// a given (Type, Id, Version) triple — safe to use as a dedupe key in
    /// projections and as a deterministic routing key on the event fabric.
    /// </summary>
    public string ToUrn()
        => $"urn:content:{Type.ToString().ToLowerInvariant()}:{Id.Value:D}:{Version.Value}";

    /// <summary>
    /// True when two refs point at the same logical content regardless of
    /// version. Used by engagement / interaction aggregates to keep comments,
    /// reactions, views, and message attachments stable across content
    /// revisions.
    /// </summary>
    public bool IsSameLogicalContent(ContentRef other)
        => Type == other.Type && Id.Value == other.Id.Value;

    public override string ToString() => ToUrn();
}
