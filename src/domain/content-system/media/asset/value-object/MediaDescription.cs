using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Asset;

public sealed record MediaDescription : ValueObject
{
    public const int MaxLength = 2_000;

    public string Value { get; }

    private MediaDescription(string value) => Value = value;

    public static MediaDescription Empty { get; } = new(string.Empty);

    public static MediaDescription Create(string? value)
    {
        var normalised = (value ?? string.Empty).Trim();
        if (normalised.Length > MaxLength)
            throw MediaAssetErrors.DescriptionTooLong(MaxLength);
        return new MediaDescription(normalised);
    }

    public override string ToString() => Value;
}
