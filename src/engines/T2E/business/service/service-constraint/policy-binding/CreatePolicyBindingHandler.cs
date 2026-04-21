using Whycespace.Domain.BusinessSystem.Service.ServiceConstraint.PolicyBinding;
using Whycespace.Shared.Contracts.Business.Service.ServiceConstraint.PolicyBinding;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Service.ServiceConstraint.PolicyBinding;

public sealed class CreatePolicyBindingHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreatePolicyBindingCommand cmd)
            return Task.CompletedTask;

        var aggregate = PolicyBindingAggregate.Create(
            new PolicyBindingId(cmd.PolicyBindingId),
            new ServiceDefinitionRef(cmd.ServiceDefinitionId),
            new PolicyRef(cmd.PolicyRef),
            (PolicyBindingScope)cmd.Scope);

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
