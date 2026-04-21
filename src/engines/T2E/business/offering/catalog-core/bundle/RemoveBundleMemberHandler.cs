using Whycespace.Domain.BusinessSystem.Offering.CatalogCore.Bundle;
using Whycespace.Shared.Contracts.Business.Offering.CatalogCore.Bundle;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Offering.CatalogCore.Bundle;

public sealed class RemoveBundleMemberHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RemoveBundleMemberCommand cmd)
            return;

        var aggregate = (BundleAggregate)await context.LoadAggregateAsync(typeof(BundleAggregate));
        var kind = Enum.Parse<BundleMemberKind>(cmd.MemberKind, ignoreCase: false);
        aggregate.RemoveMember(new BundleMember(kind, new BundleMemberId(cmd.MemberId)));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
