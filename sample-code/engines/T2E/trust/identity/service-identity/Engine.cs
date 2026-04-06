using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Primitives.Id;

namespace Whycespace.Engines.T2E.Trust.Identity.ServiceIdentity;

public sealed class ServiceIdentityEngine
{
    private readonly ServiceIdentityPolicyAdapter _policy = new();

    public async Task<EngineResult> ExecuteAsync(ServiceIdentityCommand command, EngineContext context, CancellationToken ct)
    {
        await _policy.EnforceAsync(command);

        return command switch
        {
            RegisterServiceIdentityCommand c => RegisterAsync(c),
            IssueServiceCredentialCommand c => IssueCredentialAsync(c),
            RevokeServiceCredentialCommand c => RevokeCredentialAsync(c),
            SuspendServiceIdentityCommand c => SuspendAsync(c),
            ReactivateServiceIdentityCommand c => ReactivateAsync(c),
            DecommissionServiceIdentityCommand c => DecommissionAsync(c),
            _ => throw new NotSupportedException($"Unknown command: {command.GetType().Name}")
        };
    }

    private static EngineResult RegisterAsync(RegisterServiceIdentityCommand c) => EngineResult.Ok(new ServiceIdentityDto(DeterministicIdHelper.FromSeed($"ServiceIdentity:{c.ServiceName}:{c.ServiceType}").ToString(), c.ServiceName, c.ServiceType, "Active"));
    private static EngineResult IssueCredentialAsync(IssueServiceCredentialCommand c) => EngineResult.Ok(new { c.ServiceIdentityId, Action = "CredentialIssued" });
    private static EngineResult RevokeCredentialAsync(RevokeServiceCredentialCommand c) => EngineResult.Ok(new { c.ServiceIdentityId, c.CredentialId, Action = "CredentialRevoked" });
    private static EngineResult SuspendAsync(SuspendServiceIdentityCommand c) => EngineResult.Ok(new { c.ServiceIdentityId, c.Reason, Status = "Suspended" });
    private static EngineResult ReactivateAsync(ReactivateServiceIdentityCommand c) => EngineResult.Ok(new { c.ServiceIdentityId, Status = "Active" });
    private static EngineResult DecommissionAsync(DecommissionServiceIdentityCommand c) => EngineResult.Ok(new { c.ServiceIdentityId, c.Reason, Status = "Decommissioned" });
}
