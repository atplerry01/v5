using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Primitives.Id;

namespace Whycespace.Engines.T2E.Trust.Identity.Credential;

public sealed class CredentialEngine
{
    private readonly CredentialPolicyAdapter _policy = new();

    public async Task<EngineResult> ExecuteAsync(CredentialCommand command, EngineContext context, CancellationToken ct)
    {
        await _policy.EnforceAsync(command);

        return command switch
        {
            IssueCredentialCommand c => await IssueAsync(c, context, ct),
            RevokeCredentialCommand c => RevokeAsync(c),
            RotateCredentialCommand c => RotateAsync(c),
            _ => throw new NotSupportedException($"Unknown command: {command.GetType().Name}")
        };
    }

    private async Task<EngineResult> IssueAsync(IssueCredentialCommand command, EngineContext context, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);
        var validation = await context.ValidateAsync(command.IdentityId, ct);
        if (!validation.IsValid) return EngineResult.Fail(validation.Reason ?? "Validation failed", "VALIDATION_FAILED");
        return EngineResult.Ok(new CredentialDto(DeterministicIdHelper.FromSeed($"Credential:{command.IdentityId}:{command.CredentialType}").ToString(), command.IdentityId, command.CredentialType, "Active"));
    }

    private static EngineResult RevokeAsync(RevokeCredentialCommand c) => EngineResult.Ok(new { c.CredentialId, Status = "Revoked" });
    private static EngineResult RotateAsync(RotateCredentialCommand c) => EngineResult.Ok(new { c.CredentialId, c.NewExpiryDate, Action = "Rotated" });
}
