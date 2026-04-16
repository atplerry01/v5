using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Streaming.Distribution;

public sealed record DistributionChannel : ValueObject
{
    public string Name { get; }
    private DistributionChannel(string name) => Name = name;

    public static DistributionChannel Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw DistributionErrors.InvalidChannel();
        var n = name.Trim().ToLowerInvariant();
        return new DistributionChannel(n);
    }

    public override string ToString() => Name;
}
