using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Trust.Identity.Identity;

public sealed class IdentityEngine
{
    private readonly IdentityPolicyAdapter _policy = new();

    public async Task<EngineResult> ExecuteAsync(IdentityCommand command, EngineContext context, CancellationToken ct)
    {
        await _policy.EnforceAsync(command);

        return command switch
        {
            CreateIdentityCommand c => await CreateAsync(c, context, ct),
            ActivateIdentityCommand c => ActivateAsync(c),
            SuspendIdentityCommand c => SuspendAsync(c),
            ReactivateIdentityCommand c => ReactivateAsync(c),
            DeactivateIdentityCommand c => DeactivateAsync(c),
            UpdateIdentityDisplayNameCommand c => UpdateDisplayNameAsync(c),
            _ => throw new NotSupportedException($"Unknown command: {command.GetType().Name}")
        };
    }

    private async Task<EngineResult> CreateAsync(CreateIdentityCommand command, EngineContext context, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);
        var validation = await context.ValidateAsync(command.IdentityId, ct);
        if (!validation.IsValid) return EngineResult.Fail(validation.Reason ?? "Validation failed", "VALIDATION_FAILED");
        return EngineResult.Ok(new IdentityDto(command.IdentityId, command.IdentityType, command.DisplayName, "Pending"));
    }

    private static EngineResult ActivateAsync(ActivateIdentityCommand c) => EngineResult.Ok(new { c.IdentityId, Status = "Active" });
    private static EngineResult SuspendAsync(SuspendIdentityCommand c) => EngineResult.Ok(new { c.IdentityId, c.Reason, Status = "Suspended" });
    private static EngineResult ReactivateAsync(ReactivateIdentityCommand c) => EngineResult.Ok(new { c.IdentityId, Status = "Active" });
    private static EngineResult DeactivateAsync(DeactivateIdentityCommand c) => EngineResult.Ok(new { c.IdentityId, c.Reason, Status = "Deactivated" });
    private static EngineResult UpdateDisplayNameAsync(UpdateIdentityDisplayNameCommand c) => EngineResult.Ok(new { c.IdentityId, c.NewDisplayName });
}
