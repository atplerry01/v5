namespace Whycespace.Runtime.Middleware.Policy.Loaders;

/// <summary>
/// Phase 11 B1 — narrow, policy-visible projection of
/// <c>ObligationAggregate</c> state. Serialised onto
/// <c>input.resource.state</c> with <c>PolicyInputBuilder.SerializerOptions</c>
/// (snake-case), so each C# property maps directly to a rego key the shipped
/// <c>infrastructure/policy/domain/economic/ledger/obligation.rego</c> reads:
///
///   <list type="bullet">
///     <item><c>Status</c> → <c>status</c> — primary read at obligation.rego
///       lines 97 (<c>fulfilment_state_ok</c>) and 113
///       (<c>cancellation_state_ok</c>); drives the
///       <c>obligation_already_terminal</c> deny reason.</item>
///     <item><c>Amount</c> → <c>amount</c>, <c>CounterpartyId</c> →
///       <c>counterparty_id</c>, <c>Type</c> → <c>type</c>,
///       <c>Currency</c> → <c>currency</c> — not referenced by the current
///       rego, but included for audit symmetry with the obligation's own
///       <c>ObligationCreatedEvent</c> fields so operator diagnostics can
///       see the aggregate state as it was at evaluation time.</item>
///   </list>
///
/// <para>
/// <b>Narrow surface.</b> The snapshot is a record (not the live aggregate)
/// so policy cannot reach through into aggregate internals — keeps the
/// policy boundary deterministic and replay-stable, complementary to the
/// canonical <c>POLICY-STATE-SOURCE-EVENT-STORE-01</c> fidelity contract.
/// </para>
/// </summary>
public sealed record ObligationStateSnapshot(
    string Status,
    decimal Amount,
    Guid CounterpartyId,
    string Type,
    string Currency);
