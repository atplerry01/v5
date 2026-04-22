using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Envelope.Header;

public readonly record struct HeaderKind
{
    public string Value { get; }

    public HeaderKind(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "HeaderKind cannot be empty.");
        Value = value;
    }
}
