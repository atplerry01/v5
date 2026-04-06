using Whycespace.Engines.T0U.WhyceId.Federation;
using Whycespace.Runtime.Command;
using Whycespace.Runtime.ControlPlane.Middleware;

namespace Whycespace.Engines.T0U.WhyceId.Federation;

/// <summary>
/// Runtime middleware — validates federation requests before command dispatch.
///
/// Pipeline position: AFTER authentication, BEFORE command dispatch.
///
/// Responsibilities:
///   - Extract federation payload from command context
///   - Call FederationValidationEngine
///   - Reject request if validation fails
///
/// NO engine logic — delegates to T0U engine only.
/// Uses string-based status values instead of domain enums.
/// </summary>
public sealed class FederationValidationMiddleware : IMiddleware
{
    private readonly FederationValidationEngine _validationEngine;

    public FederationValidationMiddleware(FederationValidationEngine validationEngine)
    {
        _validationEngine = validationEngine ?? throw new ArgumentNullException(nameof(validationEngine));
    }

    public async Task<CommandResult> InvokeAsync(CommandContext context, MiddlewareDelegate next)
    {
        // Only intercept federation commands
        if (!IsFederationCommand(context.Envelope.CommandType))
            return await next(context);

        var federationPayload = context.Get<FederationPayload>(ContextKeys.FederationPayload);
        if (federationPayload is null)
            return await next(context);

        var validationResult = _validationEngine.Validate(new ValidateExternalIdentityCommand(
            ExternalId: federationPayload.ExternalId,
            IssuerId: federationPayload.IssuerId,
            IssuerStatus: federationPayload.IssuerStatus,
            CredentialExpired: federationPayload.CredentialExpired,
            CredentialRevoked: federationPayload.CredentialRevoked,
            ChainVerified: true));

        // FIX 5: Emit observability signal
        context.Set(FederationObservability.Keys.ValidationResult, new FederationObservability.ValidationSignal(
            validationResult.IsValid,
            validationResult.Reasons.Count,
            validationResult.Reasons.Count > 0 ? validationResult.Reasons[0] : null));

        if (!validationResult.IsValid)
        {
            return CommandResult.Fail(
                context.Envelope.CommandId,
                $"Federation validation failed: {string.Join("; ", validationResult.Reasons)}",
                "FEDERATION_VALIDATION_FAILED",
                context.Clock.UtcNowOffset);
        }

        context.Set(ContextKeys.ValidationPassed, true);
        return await next(context);
    }

    private static bool IsFederationCommand(string commandType) =>
        commandType.StartsWith("federation.", StringComparison.OrdinalIgnoreCase) ||
        commandType.Contains(".federation.", StringComparison.OrdinalIgnoreCase);

    public static class ContextKeys
    {
        public const string FederationPayload = "Federation.Payload";
        public const string ValidationPassed = "Federation.ValidationPassed";
    }
}

/// <summary>
/// Federation payload extracted from command context for middleware processing.
/// Uses string-based status instead of domain IssuerStatus enum.
/// </summary>
public sealed record FederationPayload
{
    public required string ExternalId { get; init; }
    public required Guid IssuerId { get; init; }
    public required string IssuerStatus { get; init; }
    public required bool CredentialExpired { get; init; }
    public required bool CredentialRevoked { get; init; }
}
