namespace Whycespace.Domain.SharedKernel.Primitive.Identity;

/// Canonical identifier for content aggregates (DocumentAggregate and its
/// kin in content-system). Typed wrapper around a Guid, enforcing non-empty.
/// Local `DocumentRef` value objects in other domains wrap `ContentId` to
/// maintain strong-typed content identity at cross-BC boundaries.
public readonly record struct ContentId
{
    public Guid Value { get; }

    public ContentId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ContentId cannot be empty.", nameof(value));

        Value = value;
    }

    public static bool TryParse(string? text, out ContentId contentId)
    {
        contentId = default;
        if (!Guid.TryParse(text, out var guid) || guid == Guid.Empty)
            return false;

        contentId = new ContentId(guid);
        return true;
    }

    public override string ToString() => Value.ToString();
}
