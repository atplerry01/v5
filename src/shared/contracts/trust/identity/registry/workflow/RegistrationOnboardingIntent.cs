namespace Whycespace.Shared.Contracts.Trust.Identity.Registry.Workflow;

public sealed record RegistrationOnboardingIntent(
    Guid RegistryId,
    string Email,
    string RegistrationType);
