using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Primitives.Id;

namespace Whycespace.Engines.T2E.Trust.Identity.Device;

public sealed class DeviceEngine
{
    private readonly DevicePolicyAdapter _policy = new();

    public async Task<EngineResult> ExecuteAsync(DeviceCommand command, EngineContext context, CancellationToken ct)
    {
        await _policy.EnforceAsync(command);

        return command switch
        {
            RegisterDeviceCommand c => await RegisterAsync(c, context, ct),
            VerifyDeviceCommand c => VerifyAsync(c),
            BlockDeviceCommand c => BlockAsync(c),
            UnblockDeviceCommand c => UnblockAsync(c),
            DeregisterDeviceCommand c => DeregisterAsync(c),
            _ => throw new NotSupportedException($"Unknown command: {command.GetType().Name}")
        };
    }

    private async Task<EngineResult> RegisterAsync(RegisterDeviceCommand command, EngineContext context, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);
        var validation = await context.ValidateAsync(command.IdentityId, ct);
        if (!validation.IsValid) return EngineResult.Fail(validation.Reason ?? "Validation failed", "VALIDATION_FAILED");
        return EngineResult.Ok(new DeviceDto(DeterministicIdHelper.FromSeed($"Device:{command.IdentityId}:{command.DeviceType}:{command.Fingerprint}").ToString(), command.IdentityId, command.DeviceType, "Registered"));
    }

    private static EngineResult VerifyAsync(VerifyDeviceCommand c) => EngineResult.Ok(new { c.DeviceId, Status = "Verified" });
    private static EngineResult BlockAsync(BlockDeviceCommand c) => EngineResult.Ok(new { c.DeviceId, c.Reason, Status = "Blocked" });
    private static EngineResult UnblockAsync(UnblockDeviceCommand c) => EngineResult.Ok(new { c.DeviceId, Status = "Verified" });
    private static EngineResult DeregisterAsync(DeregisterDeviceCommand c) => EngineResult.Ok(new { c.DeviceId, c.Reason, Status = "Deregistered" });
}
