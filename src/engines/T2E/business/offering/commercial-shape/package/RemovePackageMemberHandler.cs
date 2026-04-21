using Whycespace.Domain.BusinessSystem.Offering.CommercialShape.Package;
using Whycespace.Shared.Contracts.Business.Offering.CommercialShape.Package;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Offering.CommercialShape.Package;

public sealed class RemovePackageMemberHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RemovePackageMemberCommand cmd)
            return;

        if (!Enum.TryParse<PackageMemberKind>(cmd.MemberKind, ignoreCase: true, out var kind))
            throw new InvalidOperationException(
                $"PackageMemberKind '{cmd.MemberKind}' is not a valid member kind.");

        var aggregate = (PackageAggregate)await context.LoadAggregateAsync(typeof(PackageAggregate));
        aggregate.RemoveMember(new PackageMember(kind, new PackageMemberId(cmd.MemberId)));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
