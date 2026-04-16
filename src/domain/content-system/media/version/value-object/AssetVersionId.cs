namespace Whycespace.Domain.ContentSystem.Media.Version;

public readonly record struct AssetVersionId(Guid Value)
{
    public static AssetVersionId From(Guid value) => new(value);
    public override string ToString() => Value.ToString("D");
}
