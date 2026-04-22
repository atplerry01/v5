using Whycespace.Domain.TrustSystem.Identity.Profile;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Trust.Identity.Profile;

namespace Whycespace.Engines.T2E.Trust.Identity.Profile;

public sealed class ActivateProfileHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ActivateProfileCommand)
            return;

        var aggregate = (ProfileAggregate)await context.LoadAggregateAsync(typeof(ProfileAggregate));
        aggregate.Activate();
        context.EmitEvents(aggregate.DomainEvents);
    }
}
