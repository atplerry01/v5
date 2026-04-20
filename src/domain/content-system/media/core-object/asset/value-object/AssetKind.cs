namespace Whycespace.Domain.ContentSystem.Media.CoreObject.Asset;

// Discriminator for the intrinsic type-specific descriptors absorbed into
// AssetAggregate per §CD-03. `Other` is the default for assets whose kind
// has not yet been assigned via AssignKind.
public enum AssetKind
{
    Other,
    Audio,
    Video,
    Image
}
