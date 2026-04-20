using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.CoreObject.Bundle;

public readonly record struct BundleName
{
    public string Value { get; }

    public BundleName(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "BundleName cannot be empty.");
        Guard.Against(value.Length > 256, "BundleName cannot exceed 256 characters.");
        Value = value.Trim();
    }

    public override string ToString() => Value;
}
