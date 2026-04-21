using Whycespace.Domain.BusinessSystem.Customer.IdentityAndProfile.Profile;
using Whycespace.Shared.Contracts.Business.Customer.IdentityAndProfile.Profile;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Customer.IdentityAndProfile.Profile;

public sealed class CreateProfileHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateProfileCommand cmd)
            return Task.CompletedTask;

        var aggregate = ProfileAggregate.Create(
            new ProfileId(cmd.ProfileId),
            new CustomerRef(cmd.CustomerId),
            new ProfileDisplayName(cmd.DisplayName));
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
