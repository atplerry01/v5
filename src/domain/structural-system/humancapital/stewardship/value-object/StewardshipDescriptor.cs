using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Humancapital.Stewardship;

public readonly record struct StewardshipDescriptor
{
    public string Name { get; }
    public string Kind { get; }

    public StewardshipDescriptor(string name, string kind)
    {
        Guard.Against(string.IsNullOrWhiteSpace(name), "StewardshipDescriptor name must not be empty.");
        Guard.Against(string.IsNullOrWhiteSpace(kind), "StewardshipDescriptor kind must not be empty.");

        Name = name;
        Kind = kind;
    }
}
