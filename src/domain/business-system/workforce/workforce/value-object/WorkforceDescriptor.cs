using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Workforce.Workforce;

public readonly record struct WorkforceDescriptor
{
    public string Name { get; }
    public string Kind { get; }

    public WorkforceDescriptor(string name, string kind)
    {
        Guard.Against(string.IsNullOrWhiteSpace(name), "WorkforceDescriptor name must not be empty.");
        Guard.Against(string.IsNullOrWhiteSpace(kind), "WorkforceDescriptor kind must not be empty.");

        Name = name;
        Kind = kind;
    }
}
