using Whycespace.Domain.ControlSystem.SystemPolicy.PolicyDefinition;
using Whycespace.Shared.Contracts.Control.SystemPolicy.PolicyDefinition;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Control.SystemPolicy.PolicyDefinition;

public sealed class DefinePolicyHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not DefinePolicyCommand cmd)
            return Task.CompletedTask;

        var aggregate = PolicyDefinitionAggregate.Define(
            new PolicyId(cmd.PolicyId.ToString("N").PadRight(64, '0')),
            cmd.Name,
            new PolicyScope(cmd.ScopeClassification, cmd.ScopeActionMask, cmd.ScopeContext));

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
