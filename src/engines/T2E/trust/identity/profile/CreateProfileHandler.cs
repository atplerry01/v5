using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.TrustSystem.Identity.Profile;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Trust.Identity.Profile;

namespace Whycespace.Engines.T2E.Trust.Identity.Profile;

public sealed class CreateProfileHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateProfileCommand cmd)
            return Task.CompletedTask;

        var aggregate = ProfileAggregate.Create(
            new ProfileId(cmd.ProfileId),
            new ProfileDescriptor(cmd.IdentityReference, cmd.DisplayName, cmd.ProfileType),
            new Timestamp(cmd.CreatedAt));

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
