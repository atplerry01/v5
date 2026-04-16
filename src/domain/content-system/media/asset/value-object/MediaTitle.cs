using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Asset;

public sealed record MediaTitle : ValueObject
{
    public const int MaxLength = 200;

    public string Value { get; }

    private MediaTitle(string value) => Value = value;

    public static MediaTitle Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw MediaAssetErrors.InvalidTitle();
        var trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
            throw MediaAssetErrors.TitleTooLong(MaxLength);
        return new MediaTitle(trimmed);
    }

    public override string ToString() => Value;
}
