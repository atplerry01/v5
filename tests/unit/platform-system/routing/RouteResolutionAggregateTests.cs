using Whycespace.Domain.PlatformSystem.Routing.RouteResolution;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.PlatformSystem.Routing;

public sealed class RouteResolutionAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp Now = new(new DateTimeOffset(2026, 4, 22, 10, 0, 0, TimeSpan.Zero));
    private static readonly DomainRoute SourceRoute = new("economic", "capital", "account");
    private static readonly IReadOnlyList<Guid> SomeRules = [new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")];
    private static readonly Guid SomeRouteRef = new("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    private static ResolutionId NewId(string seed) =>
        new(IdGen.Generate($"RouteResolutionAggregateTests:{seed}"));

    [Fact]
    public void Resolve_WithValidArgs_RaisesRouteResolvedEvent()
    {
        var aggregate = RouteResolutionAggregate.Resolve(
            NewId("Resolve"), SourceRoute, "CreateAccount", SomeRouteRef,
            ResolutionStrategy.ExactMatch, SomeRules, Now);

        Assert.Equal(ResolutionOutcome.Resolved, aggregate.Outcome);
        Assert.Equal(SomeRouteRef, aggregate.ResolvedRouteRef);

        var evt = Assert.IsType<RouteResolvedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal("ExactMatch", evt.Strategy.Value);
        Assert.Equal("CreateAccount", evt.MessageType);
    }

    [Fact]
    public void Fail_WithValidArgs_RaisesRouteResolutionFailedEvent()
    {
        var aggregate = RouteResolutionAggregate.Fail(
            NewId("Fail"), SourceRoute, "CreateAccount", SomeRules, "no matching rule", Now);

        Assert.Equal(ResolutionOutcome.Failed, aggregate.Outcome);
        Assert.Null(aggregate.ResolvedRouteRef);

        var evt = Assert.IsType<RouteResolutionFailedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal("no matching rule", evt.FailureReason);
    }

    [Fact]
    public void Resolve_WithInvalidSourceRoute_Throws()
    {
        var invalidRoute = new DomainRoute("", "capital", "account");

        Assert.Throws<DomainInvariantViolationException>(() =>
            RouteResolutionAggregate.Resolve(NewId("InvalidRoute"), invalidRoute, "MsgType", SomeRouteRef, ResolutionStrategy.ExactMatch, SomeRules, Now));
    }

    [Fact]
    public void Resolve_WithEmptyMessageType_Throws()
    {
        Assert.Throws<DomainInvariantViolationException>(() =>
            RouteResolutionAggregate.Resolve(NewId("EmptyMsgType"), SourceRoute, "", SomeRouteRef, ResolutionStrategy.ExactMatch, SomeRules, Now));
    }

    [Fact]
    public void Resolve_WithNoDispatchRules_Throws()
    {
        Assert.Throws<DomainInvariantViolationException>(() =>
            RouteResolutionAggregate.Resolve(NewId("NoRules"), SourceRoute, "MsgType", SomeRouteRef, ResolutionStrategy.ExactMatch, [], Now));
    }

    [Fact]
    public void Fail_WithNoDispatchRules_Throws()
    {
        Assert.Throws<DomainInvariantViolationException>(() =>
            RouteResolutionAggregate.Fail(NewId("FailNoRules"), SourceRoute, "MsgType", [], "reason", Now));
    }
}
