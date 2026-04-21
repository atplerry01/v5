using Whycespace.Domain.BusinessSystem.Customer.IdentityAndProfile.Profile;
using Whycespace.Shared.Contracts.Business.Customer.IdentityAndProfile.Profile;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Customer.IdentityAndProfile.Profile;

public sealed class ArchiveProfileHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ArchiveProfileCommand)
            return;

        var aggregate = (ProfileAggregate)await context.LoadAggregateAsync(typeof(ProfileAggregate));
        aggregate.Archive();
        context.EmitEvents(aggregate.DomainEvents);
    }
}
