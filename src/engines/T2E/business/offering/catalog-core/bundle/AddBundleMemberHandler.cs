using Whycespace.Domain.BusinessSystem.Offering.CatalogCore.Bundle;
using Whycespace.Shared.Contracts.Business.Offering.CatalogCore.Bundle;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Offering.CatalogCore.Bundle;

public sealed class AddBundleMemberHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not AddBundleMemberCommand cmd)
            return;

        var aggregate = (BundleAggregate)await context.LoadAggregateAsync(typeof(BundleAggregate));
        // MemberKind is transported as the enum name so the wire schema stays
        // decoupled from the BundleMemberKind CLR type. The domain value-object
        // guard will re-validate the enum value; an unknown name fails loudly.
        var kind = Enum.Parse<BundleMemberKind>(cmd.MemberKind, ignoreCase: false);
        aggregate.AddMember(new BundleMember(kind, new BundleMemberId(cmd.MemberId)));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
