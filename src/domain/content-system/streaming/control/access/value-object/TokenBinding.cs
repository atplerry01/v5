using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.Control.Access;

public readonly record struct TokenBinding
{
    public string Value { get; }

    public TokenBinding(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "TokenBinding cannot be empty.");
        Guard.Against(value.Length > 512, "TokenBinding cannot exceed 512 characters.");
        Value = value.Trim();
    }

    public override string ToString() => Value;
}
