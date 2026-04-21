using Whycespace.Domain.BusinessSystem.Service.ServiceConstraint.PolicyBinding;
using Whycespace.Shared.Contracts.Business.Service.ServiceConstraint.PolicyBinding;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Service.ServiceConstraint.PolicyBinding;

public sealed class UnbindPolicyBindingHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not UnbindPolicyBindingCommand cmd)
            return;

        var aggregate = (PolicyBindingAggregate)await context.LoadAggregateAsync(typeof(PolicyBindingAggregate));
        aggregate.Unbind(cmd.UnboundAt);
        context.EmitEvents(aggregate.DomainEvents);
    }
}
