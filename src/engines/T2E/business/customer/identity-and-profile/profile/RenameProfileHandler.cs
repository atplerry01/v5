using Whycespace.Domain.BusinessSystem.Customer.IdentityAndProfile.Profile;
using Whycespace.Shared.Contracts.Business.Customer.IdentityAndProfile.Profile;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Customer.IdentityAndProfile.Profile;

public sealed class RenameProfileHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RenameProfileCommand cmd)
            return;

        var aggregate = (ProfileAggregate)await context.LoadAggregateAsync(typeof(ProfileAggregate));
        aggregate.Rename(new ProfileDisplayName(cmd.DisplayName));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
