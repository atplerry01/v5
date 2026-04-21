using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Humancapital.Operator;

public readonly record struct OperatorDescriptor
{
    public string Name { get; }
    public string Kind { get; }

    public OperatorDescriptor(string name, string kind)
    {
        Guard.Against(string.IsNullOrWhiteSpace(name), "OperatorDescriptor name must not be empty.");
        Guard.Against(string.IsNullOrWhiteSpace(kind), "OperatorDescriptor kind must not be empty.");

        Name = name;
        Kind = kind;
    }
}
