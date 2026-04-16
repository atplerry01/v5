using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Asset;

public sealed record ContentDigest : ValueObject
{
    public const int Sha256HexLength = 64;

    public string Value { get; }

    private ContentDigest(string value) => Value = value;

    public static ContentDigest Create(string sha256Hex)
    {
        if (string.IsNullOrWhiteSpace(sha256Hex))
            throw MediaAssetErrors.InvalidContentDigest();
        var normalised = sha256Hex.Trim().ToLowerInvariant();
        if (normalised.Length != Sha256HexLength || !IsHex(normalised))
            throw MediaAssetErrors.InvalidContentDigest();
        return new ContentDigest(normalised);
    }

    private static bool IsHex(string value)
    {
        for (var i = 0; i < value.Length; i++)
        {
            var c = value[i];
            var isHex = (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f');
            if (!isHex) return false;
        }
        return true;
    }

    public override string ToString() => Value;
}
