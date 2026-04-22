using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Command.CommandRouting;

public sealed class CommandRoutingRuleAggregate : AggregateRoot
{
    public CommandRoutingRuleId CommandRoutingRuleId { get; private set; }
    public CommandTypeRef CommandTypeRef { get; private set; }
    public DomainRoute HandlerRoute { get; private set; } = null!;
    public bool IsRemoved { get; private set; }

    private CommandRoutingRuleAggregate() { }

    public static CommandRoutingRuleAggregate Register(
        CommandRoutingRuleId id,
        CommandTypeRef commandTypeRef,
        DomainRoute handlerRoute,
        Timestamp registeredAt)
    {
        var aggregate = new CommandRoutingRuleAggregate();
        if (aggregate.Version >= 0)
            throw CommandRoutingErrors.AlreadyInitialized();

        if (!handlerRoute.IsValid())
            throw CommandRoutingErrors.HandlerRouteMissing();

        aggregate.RaiseDomainEvent(new CommandRoutingRegisteredEvent(
            id, commandTypeRef, handlerRoute, registeredAt));

        return aggregate;
    }

    public void Remove(Timestamp removedAt)
    {
        if (IsRemoved)
            throw CommandRoutingErrors.AlreadyRemoved();

        RaiseDomainEvent(new CommandRoutingRemovedEvent(CommandRoutingRuleId, removedAt));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case CommandRoutingRegisteredEvent e:
                CommandRoutingRuleId = e.CommandRoutingRuleId;
                CommandTypeRef = e.CommandTypeRef;
                HandlerRoute = e.HandlerRoute;
                IsRemoved = false;
                break;

            case CommandRoutingRemovedEvent:
                IsRemoved = true;
                break;
        }
    }
}
