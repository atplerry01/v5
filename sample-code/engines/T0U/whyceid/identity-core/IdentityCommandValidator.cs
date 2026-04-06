using Whycespace.Engines.T0U.WhyceId.IdentityCore;
using Whycespace.Runtime.Command;
using Whycespace.Runtime.ControlPlane.Middleware;

namespace Whycespace.Engines.T0U.WhyceId;

/// <summary>
/// Command validator for identity commands. Delegates to T0U WhyceId
/// IdentityValidationEngine for pre-execution validation.
///
/// Because ICommandValidator maps 1:1 with a CommandType string, this class
/// provides a factory to create one validator per identity command type.
/// </summary>
public sealed class IdentityCommandValidator : ICommandValidator
{
    private readonly IdentityValidationEngine _validationEngine;
    private readonly string _commandType;

    private IdentityCommandValidator(IdentityValidationEngine validationEngine, string commandType)
    {
        _validationEngine = validationEngine;
        _commandType = commandType;
    }

    public string CommandType => _commandType;

    public Task<CommandValidationResult> ValidateAsync(
        CommandContext context,
        CancellationToken cancellationToken = default)
    {
        var aggregateId = context.Envelope.AggregateId ?? context.Envelope.CommandId.ToString();
        var result = ResolveValidation(_commandType, aggregateId);

        return Task.FromResult(result.IsValid
            ? CommandValidationResult.Valid()
            : CommandValidationResult.Invalid(result.Reason ?? "Validation failed", "IDENTITY_VALIDATION_FAILED"));
    }

    private IdentityValidationResult ResolveValidation(string commandType, string entityId)
    {
        var resource = commandType.Contains('.')
            ? commandType[..commandType.IndexOf('.')]
            : commandType;

        return resource.ToLowerInvariant() switch
        {
            "identity" => _validationEngine.ValidateIdentityCreate(new ValidateIdentityCreateCommand(entityId)),
            "permission" => _validationEngine.ValidatePermissionCreate(new ValidatePermissionCreateCommand(entityId)),
            "role" => _validationEngine.ValidateRoleCreate(new ValidateRoleCreateCommand(entityId)),
            "trust" => _validationEngine.ValidateTrustCreate(new ValidateTrustCreateCommand(entityId)),
            "verification" => _validationEngine.ValidateVerificationCreate(new ValidateVerificationCreateCommand(entityId)),
            "consent" => _validationEngine.ValidateConsentCreate(new ValidateConsentCreateCommand(entityId)),
            "session" => _validationEngine.ValidateSessionCreate(new ValidateSessionCreateCommand(entityId)),
            "device" => _validationEngine.ValidateDeviceCreate(new ValidateDeviceCreateCommand(entityId)),
            "service-identity" => _validationEngine.ValidateServiceIdentityCreate(new ValidateServiceIdentityCreateCommand(entityId)),
            "identity-graph" => _validationEngine.ValidateIdentityGraphCreate(new ValidateIdentityGraphCreateCommand(entityId)),
            _ => IdentityValidationResult.Valid()
        };
    }

    /// <summary>
    /// Creates one ICommandValidator per identity command type from the manifest.
    /// Register these with CommandValidationMiddleware during startup.
    /// </summary>
    public static IReadOnlyList<ICommandValidator> CreateAll(IdentityValidationEngine validationEngine)
    {
        ArgumentNullException.ThrowIfNull(validationEngine);

        return IdentityEngineManifest.GetEntries()
            .Select(entry => (ICommandValidator)new IdentityCommandValidator(validationEngine, entry.CommandType))
            .ToList();
    }
}
