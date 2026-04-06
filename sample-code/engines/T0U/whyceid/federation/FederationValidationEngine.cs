namespace Whycespace.Engines.T0U.WhyceId.Federation;

/// <summary>
/// T0U Constitutional Engine — validates external identity claims before linking.
///
/// Rules:
///   - Issuer MUST be approved
///   - Issuer MUST NOT be revoked
///   - Credential MUST NOT be expired
///   - Credential MUST NOT be revoked
///
/// Stateless. No persistence. No HTTP. Deterministic.
/// Uses string-based status values instead of domain enums.
/// </summary>
public sealed class FederationValidationEngine
{
    public FederationValidationResult Validate(ValidateExternalIdentityCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (!command.ChainVerified)
            throw new InvalidOperationException(
                "CHAIN_VERIFICATION_REQUIRED: Federation validation rejects unverified input.");

        var reasons = new List<string>();

        if (command.IssuerStatus == IssuerStatusValues.Revoked)
            reasons.Add($"Issuer '{command.IssuerId}' is revoked.");

        if (command.IssuerStatus != IssuerStatusValues.Approved)
            reasons.Add($"Issuer '{command.IssuerId}' is not approved (status: {command.IssuerStatus}).");

        if (command.CredentialExpired)
            reasons.Add("Credential is expired.");

        if (command.CredentialRevoked)
            reasons.Add("Credential has been revoked.");

        if (string.IsNullOrWhiteSpace(command.ExternalId))
            reasons.Add("External identity ID is empty.");

        return reasons.Count == 0
            ? FederationValidationResult.Valid()
            : FederationValidationResult.Invalid(reasons.ToArray());
    }
}
