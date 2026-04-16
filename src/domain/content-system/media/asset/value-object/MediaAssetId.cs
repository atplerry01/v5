namespace Whycespace.Domain.ContentSystem.Media.Asset;

public readonly record struct MediaAssetId(Guid Value)
{
    public static MediaAssetId From(Guid value) => new(value);

    public override string ToString() => Value.ToString("D");
}
