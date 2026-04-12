using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Compliance.Audit;

public readonly record struct SourceDomain
{
    public string Value { get; }

    public SourceDomain(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "SourceDomain cannot be empty.");
        Value = value;
    }
}