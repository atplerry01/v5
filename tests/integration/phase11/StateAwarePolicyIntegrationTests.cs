using System.Text.Json;
using Whycespace.Domain.EconomicSystem.Ledger.Obligation;
using Whycespace.Domain.EconomicSystem.Ledger.Treasury;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Engines.T0U.WhyceId.Engine;
using Whycespace.Engines.T0U.WhycePolicy;
using Whycespace.Engines.T0U.WhycePolicy.Engine;
using Whycespace.Runtime.Middleware.Policy;
using Whycespace.Runtime.Middleware.Policy.Loaders;
using Whycespace.Shared.Contracts.Economic.Ledger.Obligation;
using Whycespace.Shared.Contracts.Economic.Ledger.Treasury;
using Whycespace.Shared.Contracts.Infrastructure.Policy;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Tests.Integration.Setup;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Integration.Phase11;

/// <summary>
/// Phase 11 B4 — end-to-end validation that <c>obligation.rego</c> and
/// <c>treasury.rego</c> now see authoritative aggregate state via
/// <c>input.resource.state</c>, and that the stricter state-aware deny
/// rules propagate back onto <see cref="CommandResult.PolicyDenyReason"/>.
///
/// <para>
/// Each test drives <see cref="PolicyMiddleware"/> directly (bypassing the
/// full 9-stage pipeline, which would route by domain and would require
/// additional handler wiring out of Phase 11 scope). The path under test is
/// the B6 loader-seam → builder → evaluator → deny-reason-propagation
/// chain, which is exactly what Phase 11 activates.
/// </para>
///
/// <para>
/// A <see cref="CapturingPolicyEvaluator"/> stands in for real OPA — it
/// records the enriched <see cref="PolicyContext"/> the middleware hands
/// to the evaluator and returns the decision the rego rule would have
/// returned (seeded per-test with the canonical rego deny reason). This
/// lets the test pin BOTH halves of the contract in one place:
/// (a) the snapshot loaded by <see cref="CompositeAggregateStateLoader"/>
/// reaches <c>input.resource.state</c> with the correct serialised shape;
/// (b) the deny reason flows back onto the <c>CommandResult</c>.
/// </para>
/// </summary>
public sealed class StateAwarePolicyIntegrationTests
{
    // ── Obligation (state-aware fulfil deny) ─────────────────────

    [Fact]
    public async Task Obligation_Fulfilled_FulfilCommand_Denied_WithStateSnapshot_And_PolicyDenyReason()
    {
        var obligationId = Guid.Parse("00000000-0000-0000-0000-000000000e01");
        var counterpartyId = Guid.Parse("00000000-0000-0000-0000-000000000e02");
        var createdAt = new Timestamp(new DateTimeOffset(2026, 4, 17, 12, 0, 0, TimeSpan.Zero));
        var laterAt = new Timestamp(new DateTimeOffset(2026, 4, 17, 13, 0, 0, TimeSpan.Zero));

        var eventStore = new InMemoryEventStore();
        await eventStore.AppendEventsAsync(
            obligationId,
            RawEnvelopes.Wrap(
                obligationId,
                new ObligationCreatedEvent(
                    new ObligationId(obligationId),
                    counterpartyId,
                    ObligationType.Payable,
                    new Amount(250m),
                    new Currency("USD"),
                    createdAt),
                new ObligationFulfilledEvent(
                    new ObligationId(obligationId),
                    SettlementId: Guid.Parse("00000000-0000-0000-0000-000000000e03"),
                    laterAt)),
            expectedVersion: -1);

        var compositeLoader = new CompositeAggregateStateLoader(new[]
        {
            new CompositeAggregateStateLoader.Route(
                ObligationStateLoader.Handles,
                new ObligationStateLoader(eventStore)),
        });

        var capturing = new CapturingPolicyEvaluator
        {
            // Simulate obligation.rego's deny_reason set firing for
            // "obligation_already_terminal" when state.status is Fulfilled.
            NextDecision = new PolicyDecision(
                IsAllowed: false,
                PolicyId: "whyce.economic.ledger.obligation.fulfil",
                DecisionHash: "rego-obligation-already-terminal",
                DenialReason: "obligation_already_terminal")
        };

        var middleware = BuildMiddleware(capturing, compositeLoader);
        var context = BuildContext(
            aggregateId: obligationId,
            policyId: "whyce.economic.ledger.obligation.fulfil",
            classification: "economic",
            domainContext: "ledger",
            domain: "obligation");

        var command = new FulfilObligationCommand(
            obligationId,
            SettlementId: Guid.Parse("00000000-0000-0000-0000-000000000e04"));

        var handlerReached = false;
        var result = await middleware.ExecuteAsync(
            context, command,
            _ =>
            {
                handlerReached = true;
                return Task.FromResult(CommandResult.Success(Array.Empty<object>()));
            });

        // ── contract assertions ─────────────────────────────────

        Assert.False(handlerReached, "policy deny MUST stop the pipeline before the handler runs.");
        Assert.False(result.IsSuccess);
        Assert.Equal("obligation_already_terminal", result.PolicyDenyReason);
        Assert.NotNull(result.Error);
        Assert.Contains("obligation_already_terminal", result.Error!);

        var call = Assert.Single(capturing.Calls);
        Assert.Same(command, call.PolicyContext.Command);
        var snapshot = Assert.IsType<ObligationStateSnapshot>(call.PolicyContext.ResourceState);
        Assert.Equal("Fulfilled", snapshot.Status);
        Assert.Equal(250m, snapshot.Amount);
        Assert.Equal(counterpartyId, snapshot.CounterpartyId);
        Assert.Equal("Payable", snapshot.Type);
        Assert.Equal("USD", snapshot.Currency);

        // Snapshot serialises snake-case so rego's `input.resource.state.status`
        // resolves correctly. This is the wire shape OPA would see.
        var wire = JsonSerializer.Serialize(snapshot, PolicyInputBuilder.SerializerOptions);
        Assert.Contains("\"status\":\"Fulfilled\"", wire);
        Assert.Contains("\"counterparty_id\":", wire);
    }

    [Fact]
    public async Task Obligation_Pending_FulfilCommand_Allowed_WithPendingSnapshot()
    {
        // Mirror test: when status = "Pending", the rego's fulfilment_state_ok
        // branch passes, the evaluator would allow. Here we seed an allow
        // decision and assert the snapshot still reached the evaluator with
        // Status="Pending" so any future stricter rule sees it.
        var obligationId = Guid.Parse("00000000-0000-0000-0000-000000000e11");
        var createdAt = new Timestamp(new DateTimeOffset(2026, 4, 17, 12, 0, 0, TimeSpan.Zero));

        var eventStore = new InMemoryEventStore();
        await eventStore.AppendEventsAsync(
            obligationId,
            RawEnvelopes.Wrap(
                obligationId,
                new ObligationCreatedEvent(
                    new ObligationId(obligationId),
                    Guid.Parse("00000000-0000-0000-0000-000000000e12"),
                    ObligationType.Receivable,
                    new Amount(100m),
                    new Currency("EUR"),
                    createdAt)),
            expectedVersion: -1);

        var compositeLoader = new CompositeAggregateStateLoader(new[]
        {
            new CompositeAggregateStateLoader.Route(
                ObligationStateLoader.Handles,
                new ObligationStateLoader(eventStore)),
        });

        var capturing = new CapturingPolicyEvaluator();  // default: allow
        var middleware = BuildMiddleware(capturing, compositeLoader);
        var context = BuildContext(
            aggregateId: obligationId,
            policyId: "whyce.economic.ledger.obligation.fulfil",
            classification: "economic",
            domainContext: "ledger",
            domain: "obligation");

        var handlerReached = false;
        var result = await middleware.ExecuteAsync(
            context,
            new FulfilObligationCommand(
                obligationId,
                SettlementId: Guid.Parse("00000000-0000-0000-0000-000000000e14")),
            _ =>
            {
                handlerReached = true;
                return Task.FromResult(CommandResult.Success(Array.Empty<object>()));
            });

        Assert.True(handlerReached, "policy allow MUST let the pipeline continue.");
        Assert.True(result.IsSuccess, result.Error);
        Assert.Null(result.PolicyDenyReason);

        var call = Assert.Single(capturing.Calls);
        var snapshot = Assert.IsType<ObligationStateSnapshot>(call.PolicyContext.ResourceState);
        Assert.Equal("Pending", snapshot.Status);
    }

    // ── Treasury (state-aware allocate deny) ─────────────────────

    [Fact]
    public async Task Treasury_InsufficientBalance_AllocateCommand_Denied_WithStateSnapshot_And_PolicyDenyReason()
    {
        var treasuryId = Guid.Parse("00000000-0000-0000-0000-000000000f01");
        var createdAt = new Timestamp(new DateTimeOffset(2026, 4, 17, 12, 0, 0, TimeSpan.Zero));

        // Balance = 100 after Created (0) + Released 100 (=100).
        var eventStore = new InMemoryEventStore();
        await eventStore.AppendEventsAsync(
            treasuryId,
            RawEnvelopes.Wrap(
                treasuryId,
                new TreasuryCreatedEvent(
                    new TreasuryId(treasuryId),
                    new Currency("USD"),
                    createdAt),
                new TreasuryFundReleasedEvent(
                    new TreasuryId(treasuryId),
                    ReleasedAmount: new Amount(100m),
                    NewBalance: new Amount(100m))),
            expectedVersion: -1);

        var compositeLoader = new CompositeAggregateStateLoader(new[]
        {
            new CompositeAggregateStateLoader.Route(
                TreasuryStateLoader.Handles,
                new TreasuryStateLoader(eventStore)),
        });

        var capturing = new CapturingPolicyEvaluator
        {
            // Simulate treasury.rego firing "treasury_insufficient_funds"
            // when command.amount > resource.state.balance.
            NextDecision = new PolicyDecision(
                IsAllowed: false,
                PolicyId: "whyce.economic.ledger.treasury.allocate_funds",
                DecisionHash: "rego-treasury-insufficient-funds",
                DenialReason: "treasury_insufficient_funds")
        };

        var middleware = BuildMiddleware(capturing, compositeLoader);
        var context = BuildContext(
            aggregateId: treasuryId,
            policyId: "whyce.economic.ledger.treasury.allocate_funds",
            classification: "economic",
            domainContext: "ledger",
            domain: "treasury");

        var command = new AllocateFundsCommand(treasuryId, Amount: 200m);

        var handlerReached = false;
        var result = await middleware.ExecuteAsync(
            context, command,
            _ =>
            {
                handlerReached = true;
                return Task.FromResult(CommandResult.Success(Array.Empty<object>()));
            });

        Assert.False(handlerReached, "policy deny MUST stop the pipeline before the handler runs.");
        Assert.False(result.IsSuccess);
        Assert.Equal("treasury_insufficient_funds", result.PolicyDenyReason);
        Assert.NotNull(result.Error);
        Assert.Contains("treasury_insufficient_funds", result.Error!);

        var call = Assert.Single(capturing.Calls);
        Assert.Same(command, call.PolicyContext.Command);
        var snapshot = Assert.IsType<TreasuryStateSnapshot>(call.PolicyContext.ResourceState);
        Assert.Equal(100m, snapshot.Balance);
        Assert.Equal("USD", snapshot.Currency);

        // Wire-shape proof: rego reads input.resource.state.balance, so the
        // snake-case JSON must stamp "balance":100 verbatim.
        var wire = JsonSerializer.Serialize(snapshot, PolicyInputBuilder.SerializerOptions);
        Assert.Contains("\"balance\":100", wire);
    }

    // ── Helpers ──────────────────────────────────────────────────

    private static PolicyMiddleware BuildMiddleware(
        CapturingPolicyEvaluator capturing,
        IAggregateStateLoader loader) =>
        new(
            whyceIdEngine: new WhyceIdEngine(),
            whycePolicyEngine: new WhycePolicyEngine(),
            policyEvaluator: capturing,
            idGenerator: new TestIdGenerator(),
            decisionEventFactory: new PolicyDecisionEventFactory(),
            callerIdentity: new TestNoOpCallerIdentityAccessor(),
            clock: new TestClock(),
            aggregateStateLoader: loader);

    private static CommandContext BuildContext(
        Guid aggregateId,
        string policyId,
        string classification,
        string domainContext,
        string domain) => new()
        {
            CorrelationId = Guid.Parse("00000000-0000-0000-0000-0000cafe0001"),
            CausationId = Guid.Parse("00000000-0000-0000-0000-0000cafe0002"),
            CommandId = Guid.Parse("00000000-0000-0000-0000-0000cafe0003"),
            TenantId = "test-tenant",
            ActorId = "test-actor",
            AggregateId = aggregateId,
            PolicyId = policyId,
            Classification = classification,
            Context = domainContext,
            Domain = domain
        };
}
