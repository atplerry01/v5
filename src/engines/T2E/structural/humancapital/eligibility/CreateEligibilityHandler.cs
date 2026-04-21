using Whycespace.Domain.StructuralSystem.Humancapital.Eligibility;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Structural.Humancapital.Eligibility;

namespace Whycespace.Engines.T2E.Structural.Humancapital.Eligibility;

public sealed class CreateEligibilityHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateEligibilityCommand cmd) return Task.CompletedTask;
        var aggregate = EligibilityAggregate.Create(
            new EligibilityId(cmd.EligibilityId),
            new EligibilityDescriptor(cmd.Name, cmd.Kind));
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
