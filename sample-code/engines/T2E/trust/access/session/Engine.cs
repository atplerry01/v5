using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Primitives.Id;

namespace Whycespace.Engines.T2E.Trust.Access.Session;

public sealed class SessionEngine
{
    private readonly SessionPolicyAdapter _policy = new();

    public async Task<EngineResult> ExecuteAsync(SessionCommand command, EngineContext context, CancellationToken ct)
    {
        await _policy.EnforceAsync(command);

        return command switch
        {
            StartSessionCommand c => await StartAsync(c, context, ct),
            RefreshSessionCommand c => RefreshAsync(c),
            RevokeSessionCommand c => RevokeAsync(c),
            ExpireSessionCommand c => ExpireAsync(c),
            _ => throw new NotSupportedException($"Unknown command: {command.GetType().Name}")
        };
    }

    private async Task<EngineResult> StartAsync(StartSessionCommand command, EngineContext context, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);
        var validation = await context.ValidateAsync(command.IdentityId, ct);
        if (!validation.IsValid) return EngineResult.Fail(validation.Reason ?? "Validation failed", "VALIDATION_FAILED");
        return EngineResult.Ok(new SessionDto(DeterministicIdHelper.FromSeed($"Session:{command.IdentityId}:{command.DeviceId}").ToString(), command.IdentityId, command.DeviceId, "Active"));
    }

    private static EngineResult RefreshAsync(RefreshSessionCommand c) => EngineResult.Ok(new { c.SessionId, c.NewExpiresAt, Status = "Active" });
    private static EngineResult RevokeAsync(RevokeSessionCommand c) => EngineResult.Ok(new { c.SessionId, Status = "Revoked" });
    private static EngineResult ExpireAsync(ExpireSessionCommand c) => EngineResult.Ok(new { c.SessionId, Status = "Expired" });
}
