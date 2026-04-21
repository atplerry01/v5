using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Humancapital.Governance;

public readonly record struct GovernanceDescriptor
{
    public string Name { get; }
    public string Kind { get; }

    public GovernanceDescriptor(string name, string kind)
    {
        Guard.Against(string.IsNullOrWhiteSpace(name), "GovernanceDescriptor name must not be empty.");
        Guard.Against(string.IsNullOrWhiteSpace(kind), "GovernanceDescriptor kind must not be empty.");

        Name = name;
        Kind = kind;
    }
}
