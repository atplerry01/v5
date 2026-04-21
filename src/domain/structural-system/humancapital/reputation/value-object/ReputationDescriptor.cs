using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Humancapital.Reputation;

public readonly record struct ReputationDescriptor
{
    public string Name { get; }
    public string Kind { get; }

    public ReputationDescriptor(string name, string kind)
    {
        Guard.Against(string.IsNullOrWhiteSpace(name), "ReputationDescriptor name must not be empty.");
        Guard.Against(string.IsNullOrWhiteSpace(kind), "ReputationDescriptor kind must not be empty.");

        Name = name;
        Kind = kind;
    }
}
