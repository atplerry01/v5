namespace Whycespace.Domain.ContentSystem.Shared.ContentRef;

/// <summary>
/// Content categories referenceable via <see cref="ContentRef"/> across
/// bounded contexts in content-system. Extended only when a new content
/// kind genuinely needs cross-domain addressing — <see cref="Collection"/>
/// is included so courses / playlists / albums are addressable as a single
/// unit for entitlement / delivery / engagement purposes.
/// </summary>
public enum ContentType
{
    Document = 1,
    Media = 2,
    Collection = 3,
}
