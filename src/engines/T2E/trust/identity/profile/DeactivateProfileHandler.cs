using Whycespace.Domain.TrustSystem.Identity.Profile;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Trust.Identity.Profile;

namespace Whycespace.Engines.T2E.Trust.Identity.Profile;

public sealed class DeactivateProfileHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not DeactivateProfileCommand)
            return;

        var aggregate = (ProfileAggregate)await context.LoadAggregateAsync(typeof(ProfileAggregate));
        aggregate.Deactivate();
        context.EmitEvents(aggregate.DomainEvents);
    }
}
