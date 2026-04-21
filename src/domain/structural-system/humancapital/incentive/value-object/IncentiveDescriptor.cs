using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Humancapital.Incentive;

public readonly record struct IncentiveDescriptor
{
    public string Name { get; }
    public string Kind { get; }

    public IncentiveDescriptor(string name, string kind)
    {
        Guard.Against(string.IsNullOrWhiteSpace(name), "IncentiveDescriptor name must not be empty.");
        Guard.Against(string.IsNullOrWhiteSpace(kind), "IncentiveDescriptor kind must not be empty.");

        Name = name;
        Kind = kind;
    }
}
