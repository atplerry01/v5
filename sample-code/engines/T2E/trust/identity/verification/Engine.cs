using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Primitives.Id;

namespace Whycespace.Engines.T2E.Trust.Identity.Verification;

public sealed class VerificationEngine
{
    private readonly VerificationPolicyAdapter _policy = new();

    public async Task<EngineResult> ExecuteAsync(VerificationCommand command, EngineContext context, CancellationToken ct)
    {
        await _policy.EnforceAsync(command);

        return command switch
        {
            CreateVerificationCommand c => await CreateAsync(c, context, ct),
            AddVerificationAttemptCommand c => AddAttemptAsync(c),
            CompleteVerificationCommand c => CompleteAsync(c),
            FailVerificationCommand c => FailAsync(c),
            ExpireVerificationCommand c => ExpireAsync(c),
            _ => throw new NotSupportedException($"Unknown command: {command.GetType().Name}")
        };
    }

    private async Task<EngineResult> CreateAsync(CreateVerificationCommand command, EngineContext context, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);
        var validation = await context.ValidateAsync(command.IdentityId, ct);
        if (!validation.IsValid) return EngineResult.Fail(validation.Reason ?? "Validation failed", "VALIDATION_FAILED");
        return EngineResult.Ok(new VerificationDto(DeterministicIdHelper.FromSeed($"Verification:{command.IdentityId}:{command.VerificationType}:{command.Method}").ToString(), command.IdentityId, command.VerificationType, "Pending", 0));
    }

    private static EngineResult AddAttemptAsync(AddVerificationAttemptCommand c) => EngineResult.Ok(new { c.VerificationId, c.Evidence });
    private static EngineResult CompleteAsync(CompleteVerificationCommand c) => EngineResult.Ok(new { c.VerificationId, Status = "Completed" });
    private static EngineResult FailAsync(FailVerificationCommand c) => EngineResult.Ok(new { c.VerificationId, c.Reason, Status = "Failed" });
    private static EngineResult ExpireAsync(ExpireVerificationCommand c) => EngineResult.Ok(new { c.VerificationId, Status = "Expired" });
}
