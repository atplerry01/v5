using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Schema.Serialization;

public sealed record SerializationOption
{
    public string Key { get; }
    public string Value { get; }

    public SerializationOption(string key, string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(key), "SerializationOption Key cannot be empty.");
        Key = key;
        Value = value ?? string.Empty;
    }
}
