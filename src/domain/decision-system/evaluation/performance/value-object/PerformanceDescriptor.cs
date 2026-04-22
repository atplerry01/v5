using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.DecisionSystem.Evaluation.Performance;

public readonly record struct PerformanceDescriptor
{
    public string Name { get; }
    public string Kind { get; }

    public PerformanceDescriptor(string name, string kind)
    {
        Guard.Against(string.IsNullOrWhiteSpace(name), "PerformanceDescriptor name must not be empty.");
        Guard.Against(string.IsNullOrWhiteSpace(kind), "PerformanceDescriptor kind must not be empty.");

        Name = name;
        Kind = kind;
    }
}
