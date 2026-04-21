using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Humancapital.Eligibility;

public readonly record struct EligibilityDescriptor
{
    public string Name { get; }
    public string Kind { get; }

    public EligibilityDescriptor(string name, string kind)
    {
        Guard.Against(string.IsNullOrWhiteSpace(name), "EligibilityDescriptor name must not be empty.");
        Guard.Against(string.IsNullOrWhiteSpace(kind), "EligibilityDescriptor kind must not be empty.");

        Name = name;
        Kind = kind;
    }
}
