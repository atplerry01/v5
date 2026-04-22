using Whycespace.Domain.ControlSystem.AccessControl.AccessPolicy;
using Whycespace.Shared.Contracts.Control.AccessControl.AccessPolicy;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Control.AccessControl.AccessPolicy;

public sealed class DefineAccessPolicyHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not DefineAccessPolicyCommand cmd)
            return Task.CompletedTask;

        var aggregate = AccessPolicyAggregate.Define(
            new AccessPolicyId(cmd.PolicyId.ToString("N").PadRight(64, '0')),
            cmd.Name,
            cmd.Scope,
            cmd.AllowedRoleIds);

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
