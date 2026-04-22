using Microsoft.Extensions.DependencyInjection;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Contracts.Trust.Access.Session;
using Whycespace.Shared.Contracts.Trust.Identity.Consent;
using Whycespace.Shared.Contracts.Trust.Identity.Credential;
using Whycespace.Shared.Contracts.Trust.Identity.Profile;
using Whycespace.Shared.Contracts.Trust.Identity.Registry;
using Whycespace.Shared.Contracts.Trust.Identity.Verification;

namespace Whycespace.Platform.Host.Composition.Trust;

/// <summary>
/// Trust-system policy bindings. Registers one <see cref="CommandPolicyBinding"/>
/// per trust command, mapping the command CLR type to its canonical policy id constant.
/// The bindings are aggregated by <see cref="ICommandPolicyIdRegistry"/> at runtime so
/// every dispatch stamps <c>CommandContext.PolicyId</c> for evaluation by
/// <c>PolicyMiddleware</c>.
///
/// Coverage: 19 commands across 6 BCs (registry 4, profile 3, consent 3,
/// session 3, credential 3, verification 3).
/// </summary>
public static class TrustPolicyModule
{
    public static IServiceCollection AddTrustPolicyBindings(this IServiceCollection services)
    {
        // ── registry (5) ──────────────────────────────────────────────
        services.AddSingleton(new CommandPolicyBinding(typeof(InitiateRegistrationCommand), RegistryPolicyIds.Initiate));
        services.AddSingleton(new CommandPolicyBinding(typeof(VerifyRegistrationCommand),   RegistryPolicyIds.Verify));
        services.AddSingleton(new CommandPolicyBinding(typeof(ActivateRegistrationCommand), RegistryPolicyIds.Activate));
        services.AddSingleton(new CommandPolicyBinding(typeof(RejectRegistrationCommand),   RegistryPolicyIds.Reject));
        services.AddSingleton(new CommandPolicyBinding(typeof(LockRegistrationCommand),     RegistryPolicyIds.Lock));

        // ── profile (3) ───────────────────────────────────────────────
        services.AddSingleton(new CommandPolicyBinding(typeof(CreateProfileCommand),     ProfilePolicyIds.Create));
        services.AddSingleton(new CommandPolicyBinding(typeof(ActivateProfileCommand),   ProfilePolicyIds.Activate));
        services.AddSingleton(new CommandPolicyBinding(typeof(DeactivateProfileCommand), ProfilePolicyIds.Deactivate));

        // ── consent (3) ───────────────────────────────────────────────
        services.AddSingleton(new CommandPolicyBinding(typeof(GrantConsentCommand),  ConsentPolicyIds.Grant));
        services.AddSingleton(new CommandPolicyBinding(typeof(RevokeConsentCommand), ConsentPolicyIds.Revoke));
        services.AddSingleton(new CommandPolicyBinding(typeof(ExpireConsentCommand), ConsentPolicyIds.Expire));

        // ── session (3) ───────────────────────────────────────────────
        services.AddSingleton(new CommandPolicyBinding(typeof(OpenSessionCommand),      SessionPolicyIds.Open));
        services.AddSingleton(new CommandPolicyBinding(typeof(ExpireSessionCommand),    SessionPolicyIds.Expire));
        services.AddSingleton(new CommandPolicyBinding(typeof(TerminateSessionCommand), SessionPolicyIds.Terminate));

        // ── credential (3) ────────────────────────────────────────────
        services.AddSingleton(new CommandPolicyBinding(typeof(IssueCredentialCommand),    CredentialPolicyIds.Issue));
        services.AddSingleton(new CommandPolicyBinding(typeof(ActivateCredentialCommand), CredentialPolicyIds.Activate));
        services.AddSingleton(new CommandPolicyBinding(typeof(RevokeCredentialCommand),   CredentialPolicyIds.Revoke));

        // ── verification (3) ──────────────────────────────────────────
        services.AddSingleton(new CommandPolicyBinding(typeof(InitiateVerificationCommand), VerificationPolicyIds.Initiate));
        services.AddSingleton(new CommandPolicyBinding(typeof(PassVerificationCommand),     VerificationPolicyIds.Pass));
        services.AddSingleton(new CommandPolicyBinding(typeof(FailVerificationCommand),     VerificationPolicyIds.Fail));

        return services;
    }
}
