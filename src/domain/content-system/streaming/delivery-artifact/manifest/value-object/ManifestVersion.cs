using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.DeliveryArtifact.Manifest;

public readonly record struct ManifestVersion : IComparable<ManifestVersion>
{
    public int Value { get; }

    public ManifestVersion(int value)
    {
        Guard.Against(value < 1, "ManifestVersion must be >= 1.");
        Value = value;
    }

    public ManifestVersion Next() => new(Value + 1);

    public int CompareTo(ManifestVersion other) => Value.CompareTo(other.Value);

    public override string ToString() => $"v{Value}";
}
