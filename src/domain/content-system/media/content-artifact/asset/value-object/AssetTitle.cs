using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.ContentArtifact.Asset;

public readonly record struct AssetTitle
{
    public string Value { get; }

    public AssetTitle(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "AssetTitle cannot be empty or whitespace.");
        Guard.Against(value.Length > 256, "AssetTitle cannot exceed 256 characters.");
        Value = value.Trim();
    }

    public override string ToString() => Value;
}
