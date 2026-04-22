using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Workforce.Sponsorship;

public readonly record struct SponsorshipDescriptor
{
    public string Name { get; }
    public string Kind { get; }

    public SponsorshipDescriptor(string name, string kind)
    {
        Guard.Against(string.IsNullOrWhiteSpace(name), "SponsorshipDescriptor name must not be empty.");
        Guard.Against(string.IsNullOrWhiteSpace(kind), "SponsorshipDescriptor kind must not be empty.");

        Name = name;
        Kind = kind;
    }
}
