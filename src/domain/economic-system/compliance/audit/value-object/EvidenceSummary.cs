using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Compliance.Audit;

public readonly record struct EvidenceSummary
{
    public string Value { get; }

    public EvidenceSummary(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "EvidenceSummary cannot be empty.");
        Value = value;
    }
}