using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Trust.Access.Role;

public sealed class RoleEngine
{
    private readonly RolePolicyAdapter _policy = new();

    public async Task<EngineResult> ExecuteAsync(RoleCommand command, EngineContext context, CancellationToken ct)
    {
        await _policy.EnforceAsync(command);

        return command switch
        {
            CreateRoleCommand c => await CreateAsync(c, context, ct),
            AssignRoleCommand c => AssignAsync(c),
            RevokeRoleCommand c => RevokeAsync(c),
            DeactivateRoleCommand c => DeactivateAsync(c),
            _ => throw new NotSupportedException($"Unknown command: {command.GetType().Name}")
        };
    }

    private async Task<EngineResult> CreateAsync(CreateRoleCommand command, EngineContext context, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);
        var validation = await context.ValidateAsync(command.RoleId, ct);
        if (!validation.IsValid) return EngineResult.Fail(validation.Reason ?? "Validation failed", "VALIDATION_FAILED");
        return EngineResult.Ok(new RoleDto(command.RoleId, command.Name, command.Scope, "Active"));
    }

    private static EngineResult AssignAsync(AssignRoleCommand c) => EngineResult.Ok(new { c.RoleId, c.IdentityId, Action = "Assigned" });
    private static EngineResult RevokeAsync(RevokeRoleCommand c) => EngineResult.Ok(new { c.RoleId, c.IdentityId, Action = "Revoked" });
    private static EngineResult DeactivateAsync(DeactivateRoleCommand c) => EngineResult.Ok(new { c.RoleId, Status = "Deactivated" });
}
