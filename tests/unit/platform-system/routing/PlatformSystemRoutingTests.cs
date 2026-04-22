using Whycespace.Domain.PlatformSystem.Routing.DispatchRule;
using Whycespace.Domain.PlatformSystem.Routing.RouteDefinition;
using Whycespace.Domain.PlatformSystem.Routing.RouteDescriptor;
using Whycespace.Domain.PlatformSystem.Routing.RouteResolution;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Tests.Unit.PlatformSystem.Routing;

/// <summary>
/// E12 — Domain tests for the routing context: dispatch-rule, route-definition, route-descriptor, route-resolution.
/// Covers topics 6 (aggregate design), 26 (test certification) of platform-system.md.
/// </summary>
public sealed class PlatformSystemRoutingTests
{
    private static readonly Guid _id1 = new("dddddddd-0000-0000-0000-000000000001");
    private static readonly Guid _id2 = new("dddddddd-0000-0000-0000-000000000002");
    private static readonly Guid _id3 = new("dddddddd-0000-0000-0000-000000000003");
    private static readonly Timestamp _ts = new(new DateTimeOffset(2025, 6, 1, 12, 0, 0, TimeSpan.Zero));
    private static readonly DomainRoute _source = new("platform-system", "command", "command-definition");
    private static readonly DomainRoute _dest = new("economic-system", "capital", "vault");

    // -------------------------------------------------------------------------
    // DispatchRuleAggregate
    // -------------------------------------------------------------------------

    [Fact]
    public void DispatchRule_Register_ValidInputs_SetsState()
    {
        var condition = new DispatchCondition(DispatchConditionType.AlwaysMatch, string.Empty);
        var agg = DispatchRuleAggregate.Register(
            new DispatchRuleId(_id1),
            "route-all-commands",
            _id2,
            condition,
            0,
            _ts);

        Assert.Equal(_id1, agg.DispatchRuleId.Value);
        Assert.Equal("route-all-commands", agg.RuleName);
        Assert.Equal(_id2, agg.RouteRef);
        Assert.Equal(DispatchRuleStatus.Active, agg.Status);
    }

    [Fact]
    public void DispatchRule_Register_RaisesRegisteredEvent()
    {
        var condition = new DispatchCondition(DispatchConditionType.MessageKindMatch, "Command");
        var agg = DispatchRuleAggregate.Register(
            new DispatchRuleId(_id1),
            "rule-commands",
            _id2,
            condition,
            10,
            _ts);

        Assert.Single(agg.DomainEvents);
        Assert.IsType<DispatchRuleRegisteredEvent>(agg.DomainEvents[0]);
    }

    [Fact]
    public void DispatchRule_Register_EmptyRuleName_Throws()
    {
        var condition = new DispatchCondition(DispatchConditionType.AlwaysMatch, string.Empty);
        Assert.Throws<DomainInvariantViolationException>(() =>
            DispatchRuleAggregate.Register(
                new DispatchRuleId(_id1),
                "",
                _id2,
                condition,
                0,
                _ts));
    }

    [Fact]
    public void DispatchRule_Register_EmptyRouteRef_Throws()
    {
        var condition = new DispatchCondition(DispatchConditionType.AlwaysMatch, string.Empty);
        Assert.Throws<DomainInvariantViolationException>(() =>
            DispatchRuleAggregate.Register(
                new DispatchRuleId(_id1),
                "rule",
                Guid.Empty,
                condition,
                0,
                _ts));
    }

    [Fact]
    public void DispatchRule_Register_NegativePriority_Throws()
    {
        var condition = new DispatchCondition(DispatchConditionType.AlwaysMatch, string.Empty);
        Assert.Throws<DomainInvariantViolationException>(() =>
            DispatchRuleAggregate.Register(
                new DispatchRuleId(_id1),
                "rule",
                _id2,
                condition,
                -1,
                _ts));
    }

    [Fact]
    public void DispatchRule_Deactivate_ChangesStatus()
    {
        var condition = new DispatchCondition(DispatchConditionType.AlwaysMatch, string.Empty);
        var agg = DispatchRuleAggregate.Register(
            new DispatchRuleId(_id1),
            "rule",
            _id2,
            condition,
            0,
            _ts);

        agg.Deactivate(_ts);

        Assert.Equal(DispatchRuleStatus.Inactive, agg.Status);
        Assert.Equal(2, agg.DomainEvents.Count);
    }

    [Fact]
    public void DispatchCondition_AlwaysMatch_WithNonEmptyValue_Throws()
    {
        Assert.Throws<DomainInvariantViolationException>(() =>
            new DispatchCondition(DispatchConditionType.AlwaysMatch, "non-empty"));
    }

    [Fact]
    public void DispatchCondition_MessageKindMatch_WithMatchValue_Accepted()
    {
        var cond = new DispatchCondition(DispatchConditionType.MessageKindMatch, "Command");
        Assert.Equal("Command", cond.MatchValue);
    }

    // -------------------------------------------------------------------------
    // RouteDefinitionAggregate
    // -------------------------------------------------------------------------

    [Fact]
    public void RouteDefinition_Register_ValidInputs_SetsState()
    {
        var agg = RouteDefinitionAggregate.Register(
            new RouteDefinitionId(_id1),
            "vault-command-route",
            _source,
            _dest,
            TransportHint.Kafka,
            5,
            _ts);

        Assert.Equal(_id1, agg.RouteDefinitionId.Value);
        Assert.Equal("vault-command-route", agg.RouteName);
        Assert.Equal(TransportHint.Kafka, agg.TransportHint);
        Assert.Equal(RouteDefinitionStatus.Active, agg.Status);
    }

    [Fact]
    public void RouteDefinition_Register_SelfRouting_Throws()
    {
        Assert.Throws<DomainInvariantViolationException>(() =>
            RouteDefinitionAggregate.Register(
                new RouteDefinitionId(_id1),
                "self-route",
                _source,
                _source,
                TransportHint.InProcess,
                0,
                _ts));
    }

    [Fact]
    public void RouteDefinition_Register_EmptyRouteName_Throws()
    {
        Assert.Throws<DomainInvariantViolationException>(() =>
            RouteDefinitionAggregate.Register(
                new RouteDefinitionId(_id1),
                "",
                _source,
                _dest,
                TransportHint.Kafka,
                0,
                _ts));
    }

    [Fact]
    public void RouteDefinition_Register_NegativePriority_Throws()
    {
        Assert.Throws<DomainInvariantViolationException>(() =>
            RouteDefinitionAggregate.Register(
                new RouteDefinitionId(_id1),
                "route",
                _source,
                _dest,
                TransportHint.Kafka,
                -1,
                _ts));
    }

    [Fact]
    public void RouteDefinition_Deactivate_ChangesStatus()
    {
        var agg = RouteDefinitionAggregate.Register(
            new RouteDefinitionId(_id1),
            "route",
            _source,
            _dest,
            TransportHint.Kafka,
            0,
            _ts);

        agg.Deactivate(_ts);

        Assert.Equal(RouteDefinitionStatus.Inactive, agg.Status);
    }

    [Fact]
    public void RouteDefinition_Deprecate_ChangesStatus()
    {
        var agg = RouteDefinitionAggregate.Register(
            new RouteDefinitionId(_id1),
            "route",
            _source,
            _dest,
            TransportHint.Http,
            0,
            _ts);

        agg.Deprecate(_ts);

        Assert.Equal(RouteDefinitionStatus.Deprecated, agg.Status);
    }

    // -------------------------------------------------------------------------
    // RouteDescriptorAggregate
    // -------------------------------------------------------------------------

    [Fact]
    public void RouteDescriptor_Register_ValidInputs_SetsState()
    {
        var agg = RouteDescriptorAggregate.Register(
            new RouteDescriptorId(_id1),
            _source,
            _dest,
            "kafka",
            1,
            _ts);

        Assert.Equal(_id1, agg.RouteDescriptorId.Value);
        Assert.Equal(_source, agg.Source);
        Assert.Equal(_dest, agg.Destination);
        Assert.Equal("kafka", agg.TransportHint);
    }

    [Fact]
    public void RouteDescriptor_Register_SelfRouting_Throws()
    {
        Assert.Throws<DomainInvariantViolationException>(() =>
            RouteDescriptorAggregate.Register(
                new RouteDescriptorId(_id1),
                _source,
                _source,
                "kafka",
                0,
                _ts));
    }

    [Fact]
    public void RouteDescriptor_Register_EmptyTransportHint_Throws()
    {
        Assert.Throws<DomainInvariantViolationException>(() =>
            RouteDescriptorAggregate.Register(
                new RouteDescriptorId(_id1),
                _source,
                _dest,
                "",
                0,
                _ts));
    }

    // -------------------------------------------------------------------------
    // RouteResolutionAggregate
    // -------------------------------------------------------------------------

    [Fact]
    public void RouteResolution_Resolve_ValidInputs_SetsState()
    {
        var rules = new List<Guid> { _id2, _id3 };
        var agg = RouteResolutionAggregate.Resolve(
            new ResolutionId(_id1),
            _source,
            "DefineVaultCommand",
            _id3,
            ResolutionStrategy.ExactMatch,
            rules,
            _ts);

        Assert.Equal(_id1, agg.ResolutionId.Value);
        Assert.Equal(ResolutionOutcome.Resolved, agg.Outcome);
        Assert.Equal(_id3, agg.ResolvedRouteRef);
        Assert.Equal(2, agg.DispatchRulesEvaluated.Count);
    }

    [Fact]
    public void RouteResolution_Fail_ValidInputs_SetsFailedOutcome()
    {
        var rules = new List<Guid> { _id2 };
        var agg = RouteResolutionAggregate.Fail(
            new ResolutionId(_id1),
            _source,
            "UnknownCommand",
            rules,
            "no matching route found",
            _ts);

        Assert.Equal(ResolutionOutcome.Failed, agg.Outcome);
        Assert.Null(agg.ResolvedRouteRef);
    }

    [Fact]
    public void RouteResolution_Resolve_EmptyDispatchRules_Throws()
    {
        Assert.Throws<DomainInvariantViolationException>(() =>
            RouteResolutionAggregate.Resolve(
                new ResolutionId(_id1),
                _source,
                "SomeCommand",
                _id2,
                ResolutionStrategy.DefaultRoute,
                [],
                _ts));
    }

    [Fact]
    public void RouteResolution_Resolve_EmptyMessageType_Throws()
    {
        Assert.Throws<DomainInvariantViolationException>(() =>
            RouteResolutionAggregate.Resolve(
                new ResolutionId(_id1),
                _source,
                "",
                _id2,
                ResolutionStrategy.ExactMatch,
                new List<Guid> { _id3 },
                _ts));
    }

    [Fact]
    public void ResolutionStrategy_StaticValues_AreDistinct()
    {
        Assert.NotEqual(ResolutionStrategy.ExactMatch, ResolutionStrategy.PrefixMatch);
        Assert.NotEqual(ResolutionStrategy.PrefixMatch, ResolutionStrategy.DefaultRoute);
    }
}
