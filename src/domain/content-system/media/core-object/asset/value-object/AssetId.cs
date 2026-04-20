using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.CoreObject.Asset;

public readonly record struct AssetId
{
    public Guid Value { get; }

    public AssetId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "AssetId cannot be empty.");
        Value = value;
    }
}
