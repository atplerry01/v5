namespace Whycespace.Domain.ContentSystem.Media.Metadata;

public readonly record struct MetadataId(Guid Value)
{
    public static MetadataId From(Guid value) => new(value);
    public override string ToString() => Value.ToString("D");
}
