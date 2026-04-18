namespace Whycespace.Domain.ContentSystem.Shared.ContentRef;

/// <summary>
/// Deterministic identifier of a specific content entity (document, media,
/// collection). Sourced from the authoring aggregate's own id
/// (e.g., a MediaAggregate's MediaId.Value) and exposed through ContentRef
/// so consuming domains never import the authoring domain's typed id.
/// </summary>
public readonly record struct ContentId(Guid Value)
{
    public static ContentId From(Guid value) => new(value);

    public override string ToString() => Value.ToString("D");
}
