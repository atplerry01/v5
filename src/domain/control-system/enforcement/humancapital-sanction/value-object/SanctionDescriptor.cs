using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Enforcement.HumancapitalSanction;

public readonly record struct SanctionDescriptor
{
    public string Name { get; }
    public string Kind { get; }

    public SanctionDescriptor(string name, string kind)
    {
        Guard.Against(string.IsNullOrWhiteSpace(name), "SanctionDescriptor name must not be empty.");
        Guard.Against(string.IsNullOrWhiteSpace(kind), "SanctionDescriptor kind must not be empty.");

        Name = name;
        Kind = kind;
    }
}
