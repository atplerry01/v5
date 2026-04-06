using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Primitives.Id;

namespace Whycespace.Engines.T2E.Trust.Identity.Consent;

public sealed class ConsentEngine
{
    private readonly ConsentPolicyAdapter _policy = new();

    public async Task<EngineResult> ExecuteAsync(ConsentCommand command, EngineContext context, CancellationToken ct)
    {
        await _policy.EnforceAsync(command);

        return command switch
        {
            GrantConsentCommand c => await GrantAsync(c, context, ct),
            RevokeConsentCommand c => RevokeAsync(c),
            ExpireConsentCommand c => ExpireAsync(c),
            _ => throw new NotSupportedException($"Unknown command: {command.GetType().Name}")
        };
    }

    private async Task<EngineResult> GrantAsync(GrantConsentCommand command, EngineContext context, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);
        var validation = await context.ValidateAsync(command.IdentityId, ct);
        if (!validation.IsValid) return EngineResult.Fail(validation.Reason ?? "Validation failed", "VALIDATION_FAILED");
        return EngineResult.Ok(new ConsentDto(DeterministicIdHelper.FromSeed($"Consent:{command.IdentityId}:{command.ConsentType}:{command.Scope}").ToString(), command.IdentityId, command.ConsentType, command.Scope, "Granted"));
    }

    private static EngineResult RevokeAsync(RevokeConsentCommand c) => EngineResult.Ok(new { c.ConsentId, c.Reason, Status = "Revoked" });
    private static EngineResult ExpireAsync(ExpireConsentCommand c) => EngineResult.Ok(new { c.ConsentId, Status = "Expired" });
}
