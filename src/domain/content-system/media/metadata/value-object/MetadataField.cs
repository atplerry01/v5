using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Metadata;

public sealed record MetadataField : ValueObject
{
    public const int KeyMaxLength = 64;
    public const int ValueMaxLength = 2_000;

    public string Key { get; }
    public string Value { get; }

    private MetadataField(string key, string value)
    {
        Key = key;
        Value = value;
    }

    public static MetadataField Create(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw MetadataErrors.InvalidKey();
        var k = key.Trim().ToLowerInvariant();
        if (k.Length > KeyMaxLength)
            throw MetadataErrors.KeyTooLong(KeyMaxLength);
        var v = (value ?? string.Empty).Trim();
        if (v.Length > ValueMaxLength)
            throw MetadataErrors.ValueTooLong(ValueMaxLength);
        return new MetadataField(k, v);
    }

    public override string ToString() => $"{Key}={Value}";
}
