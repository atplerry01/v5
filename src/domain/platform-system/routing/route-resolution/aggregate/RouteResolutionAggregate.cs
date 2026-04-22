using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Routing.RouteResolution;

public sealed class RouteResolutionAggregate : AggregateRoot
{
    public ResolutionId ResolutionId { get; private set; }
    public DomainRoute SourceRoute { get; private set; } = null!;
    public string MessageType { get; private set; } = string.Empty;
    public Guid? ResolvedRouteRef { get; private set; }
    public ResolutionStrategy Strategy { get; private set; }
    public IReadOnlyList<Guid> DispatchRulesEvaluated { get; private set; } = [];
    public ResolutionOutcome Outcome { get; private set; }
    public Timestamp ResolvedAt { get; private set; }

    private RouteResolutionAggregate() { }

    public static RouteResolutionAggregate Resolve(
        ResolutionId id,
        DomainRoute sourceRoute,
        string messageType,
        Guid resolvedRouteRef,
        ResolutionStrategy strategy,
        IReadOnlyList<Guid> dispatchRulesEvaluated,
        Timestamp resolvedAt)
    {
        var aggregate = new RouteResolutionAggregate();
        if (aggregate.Version >= 0)
            throw RouteResolutionErrors.AlreadyInitialized();

        if (!sourceRoute.IsValid())
            throw RouteResolutionErrors.SourceRouteMissing();

        if (string.IsNullOrWhiteSpace(messageType))
            throw RouteResolutionErrors.MessageTypeMissing();

        if (dispatchRulesEvaluated is null || dispatchRulesEvaluated.Count == 0)
            throw RouteResolutionErrors.DispatchRulesEmpty();

        aggregate.RaiseDomainEvent(new RouteResolvedEvent(
            id, sourceRoute, messageType, resolvedRouteRef, strategy, dispatchRulesEvaluated, resolvedAt));

        return aggregate;
    }

    public static RouteResolutionAggregate Fail(
        ResolutionId id,
        DomainRoute sourceRoute,
        string messageType,
        IReadOnlyList<Guid> dispatchRulesEvaluated,
        string failureReason,
        Timestamp resolvedAt)
    {
        var aggregate = new RouteResolutionAggregate();
        if (aggregate.Version >= 0)
            throw RouteResolutionErrors.AlreadyInitialized();

        if (!sourceRoute.IsValid())
            throw RouteResolutionErrors.SourceRouteMissing();

        if (string.IsNullOrWhiteSpace(messageType))
            throw RouteResolutionErrors.MessageTypeMissing();

        if (dispatchRulesEvaluated is null || dispatchRulesEvaluated.Count == 0)
            throw RouteResolutionErrors.DispatchRulesEmpty();

        aggregate.RaiseDomainEvent(new RouteResolutionFailedEvent(
            id, sourceRoute, messageType, dispatchRulesEvaluated, failureReason, resolvedAt));

        return aggregate;
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case RouteResolvedEvent e:
                ResolutionId = e.ResolutionId;
                SourceRoute = e.SourceRoute;
                MessageType = e.MessageType;
                ResolvedRouteRef = e.ResolvedRouteRef;
                Strategy = e.Strategy;
                DispatchRulesEvaluated = e.DispatchRulesEvaluated;
                Outcome = ResolutionOutcome.Resolved;
                ResolvedAt = e.ResolvedAt;
                break;

            case RouteResolutionFailedEvent e:
                ResolutionId = e.ResolutionId;
                SourceRoute = e.SourceRoute;
                MessageType = e.MessageType;
                ResolvedRouteRef = null;
                DispatchRulesEvaluated = e.DispatchRulesEvaluated;
                Outcome = ResolutionOutcome.Failed;
                ResolvedAt = e.ResolvedAt;
                break;
        }
    }
}
