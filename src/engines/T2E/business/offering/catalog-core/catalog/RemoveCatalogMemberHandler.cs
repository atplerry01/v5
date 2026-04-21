using Whycespace.Domain.BusinessSystem.Offering.CatalogCore.Catalog;
using Whycespace.Shared.Contracts.Business.Offering.CatalogCore.Catalog;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Offering.CatalogCore.Catalog;

public sealed class RemoveCatalogMemberHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RemoveCatalogMemberCommand cmd)
            return;

        var aggregate = (CatalogAggregate)await context.LoadAggregateAsync(typeof(CatalogAggregate));
        var kind = Enum.Parse<CatalogMemberKind>(cmd.MemberKind, ignoreCase: false);
        aggregate.RemoveMember(new CatalogMember(kind, new CatalogMemberId(cmd.MemberId)));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
