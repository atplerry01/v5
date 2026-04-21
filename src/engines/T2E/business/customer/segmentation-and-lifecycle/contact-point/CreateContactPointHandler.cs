using Whycespace.Domain.BusinessSystem.Customer.SegmentationAndLifecycle.ContactPoint;
using Whycespace.Shared.Contracts.Business.Customer.SegmentationAndLifecycle.ContactPoint;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Customer.SegmentationAndLifecycle.ContactPoint;

public sealed class CreateContactPointHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateContactPointCommand cmd)
            return Task.CompletedTask;

        if (!Enum.TryParse<ContactPointKind>(cmd.Kind, ignoreCase: true, out var kind))
            throw new InvalidOperationException(
                $"Unknown ContactPointKind '{cmd.Kind}'.");

        var aggregate = ContactPointAggregate.Create(
            new ContactPointId(cmd.ContactPointId),
            new CustomerRef(cmd.CustomerId),
            kind,
            new ContactPointValue(cmd.Value));
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
