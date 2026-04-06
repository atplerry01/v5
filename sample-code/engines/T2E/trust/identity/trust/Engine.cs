using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Primitives.Id;

namespace Whycespace.Engines.T2E.Trust.Identity.Trust;

public sealed class TrustEngine
{
    private readonly TrustPolicyAdapter _policy = new();

    public async Task<EngineResult> ExecuteAsync(TrustCommand command, EngineContext context, CancellationToken ct)
    {
        await _policy.EnforceAsync(command);

        return command switch
        {
            InitializeTrustCommand c => await InitializeAsync(c, context, ct),
            RecordTrustFactorCommand c => RecordFactorAsync(c),
            FreezeTrustCommand c => FreezeAsync(c),
            UnfreezeTrustCommand c => UnfreezeAsync(c),
            _ => throw new NotSupportedException($"Unknown command: {command.GetType().Name}")
        };
    }

    private async Task<EngineResult> InitializeAsync(InitializeTrustCommand command, EngineContext context, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);
        var validation = await context.ValidateAsync(command.IdentityId, ct);
        if (!validation.IsValid) return EngineResult.Fail(validation.Reason ?? "Validation failed", "VALIDATION_FAILED");
        return EngineResult.Ok(new TrustDto(DeterministicIdHelper.FromSeed($"TrustProfile:{command.IdentityId}").ToString(), command.IdentityId, 0m, "Unverified", "Active"));
    }

    private static EngineResult RecordFactorAsync(RecordTrustFactorCommand c) => EngineResult.Ok(new { c.TrustProfileId, c.Factor, c.Weight });
    private static EngineResult FreezeAsync(FreezeTrustCommand c) => EngineResult.Ok(new { c.TrustProfileId, c.Reason, Status = "Frozen" });
    private static EngineResult UnfreezeAsync(UnfreezeTrustCommand c) => EngineResult.Ok(new { c.TrustProfileId, Status = "Active" });
}
