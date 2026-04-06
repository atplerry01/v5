using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Trust.Access.Permission;

public sealed class PermissionEngine
{
    private readonly PermissionPolicyAdapter _policy = new();

    public async Task<EngineResult> ExecuteAsync(PermissionCommand command, EngineContext context, CancellationToken ct)
    {
        await _policy.EnforceAsync(command);

        return command switch
        {
            CreatePermissionCommand c => await CreateAsync(c, context, ct),
            GrantPermissionCommand c => GrantAsync(c),
            RevokePermissionCommand c => RevokeAsync(c),
            DeactivatePermissionCommand c => DeactivateAsync(c),
            _ => throw new NotSupportedException($"Unknown command: {command.GetType().Name}")
        };
    }

    private async Task<EngineResult> CreateAsync(CreatePermissionCommand command, EngineContext context, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);
        var validation = await context.ValidateAsync(command.PermissionId, ct);
        if (!validation.IsValid) return EngineResult.Fail(validation.Reason ?? "Validation failed", "VALIDATION_FAILED");
        return EngineResult.Ok(new PermissionDto(command.PermissionId, command.Name, command.PermissionAction, command.Resource, "Active"));
    }

    private static EngineResult GrantAsync(GrantPermissionCommand c) =>
        EngineResult.Ok(new { c.PermissionId, c.IdentityId, Action = "Granted" });

    private static EngineResult RevokeAsync(RevokePermissionCommand c) =>
        EngineResult.Ok(new { c.PermissionId, c.IdentityId, Action = "Revoked" });

    private static EngineResult DeactivateAsync(DeactivatePermissionCommand c) =>
        EngineResult.Ok(new { c.PermissionId, Status = "Deactivated" });
}
