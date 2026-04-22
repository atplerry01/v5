using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.TrustSystem.Identity.Registry;

public readonly record struct RegistrationDescriptor
{
    public string Email { get; }
    public string RegistrationType { get; }

    public RegistrationDescriptor(string email, string registrationType)
    {
        Guard.Against(string.IsNullOrWhiteSpace(email), "RegistrationDescriptor.Email must not be empty.");
        Guard.Against(string.IsNullOrWhiteSpace(registrationType), "RegistrationDescriptor.RegistrationType must not be empty.");
        Email = email.Trim().ToLowerInvariant();
        RegistrationType = registrationType.Trim();
    }
}
