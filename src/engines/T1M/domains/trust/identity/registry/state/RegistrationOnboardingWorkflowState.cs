namespace Whycespace.Engines.T1M.Domains.Trust.Identity.Registry.State;

public static class RegistrationOnboardingSteps
{
    public const string Validate = "validate";
    public const string Verify = "verify";
    public const string Activate = "activate";
}

public sealed class RegistrationOnboardingWorkflowState
{
    public Guid RegistryId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string RegistrationType { get; init; } = string.Empty;
    public string CurrentStep { get; set; } = string.Empty;
}
