using Whycespace.Domain.StructuralSystem.Humancapital.Sponsorship;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Structural.Humancapital.Sponsorship;

namespace Whycespace.Engines.T2E.Structural.Humancapital.Sponsorship;

public sealed class CreateSponsorshipHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateSponsorshipCommand cmd) return Task.CompletedTask;
        var aggregate = SponsorshipAggregate.Create(
            new SponsorshipId(cmd.SponsorshipId),
            new SponsorshipDescriptor(cmd.Name, cmd.Kind));
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
