using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Asset;

public sealed record MediaTag : ValueObject
{
    public const int MaxLength = 48;

    public string Value { get; }

    private MediaTag(string value) => Value = value;

    public static MediaTag Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw MediaAssetErrors.InvalidTag();
        var normalised = value.Trim().ToLowerInvariant();
        if (normalised.Length > MaxLength)
            throw MediaAssetErrors.TagTooLong(MaxLength);
        return new MediaTag(normalised);
    }

    public override string ToString() => Value;
}
