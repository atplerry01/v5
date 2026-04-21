using Whycespace.Domain.BusinessSystem.Offering.CatalogCore.Catalog;
using Whycespace.Shared.Contracts.Business.Offering.CatalogCore.Catalog;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Offering.CatalogCore.Catalog;

public sealed class AddCatalogMemberHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not AddCatalogMemberCommand cmd)
            return;

        var aggregate = (CatalogAggregate)await context.LoadAggregateAsync(typeof(CatalogAggregate));
        // MemberKind is transported as the enum name so the wire schema stays
        // decoupled from the CatalogMemberKind CLR type.
        var kind = Enum.Parse<CatalogMemberKind>(cmd.MemberKind, ignoreCase: false);
        aggregate.AddMember(new CatalogMember(kind, new CatalogMemberId(cmd.MemberId)));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
