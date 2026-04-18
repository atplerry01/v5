namespace Whycespace.Domain.ContentSystem.Shared.ContentRef;

/// <summary>
/// Monotonic per-content version. A new ContentRef with Version+1 is emitted
/// when the authoring aggregate revises content in a way consuming domains
/// must observe (e.g., MediaAggregate finalizes a new variant set,
/// DocumentAggregate seals a new revision). Logical-identity comparisons
/// use <see cref="ContentRef.IsSameLogicalContent"/> to ignore version,
/// so a comment attached at v1 remains attached to the same logical
/// document at v2.
/// </summary>
public readonly record struct ContentVersion(int Value)
{
    public static ContentVersion Initial => new(1);

    public ContentVersion Next() => new(Value + 1);

    public override string ToString() => Value.ToString();
}
