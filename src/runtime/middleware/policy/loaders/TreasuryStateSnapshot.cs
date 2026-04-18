namespace Whycespace.Runtime.Middleware.Policy.Loaders;

/// <summary>
/// Phase 11 B2 — narrow, policy-visible projection of
/// <c>TreasuryAggregate</c> state. Serialised onto
/// <c>input.resource.state</c> with <c>PolicyInputBuilder.SerializerOptions</c>
/// (snake-case), so each C# property maps directly to a rego key that
/// <c>infrastructure/policy/domain/economic/ledger/treasury.rego</c> reads:
///
///   <list type="bullet">
///     <item><c>Balance</c> → <c>balance</c> — primary read at treasury.rego
///       lines 100 (<c>allocation_balance_ok</c>) and 132
///       (<c>treasury_insufficient_funds</c> deny reason).</item>
///     <item><c>Currency</c> → <c>currency</c> — included for audit
///       symmetry with the treasury's own <c>TreasuryCreatedEvent</c>
///       fields so operator diagnostics can see the aggregate state as it
///       was at evaluation time.</item>
///   </list>
///
/// Shape parity with <see cref="ObligationStateSnapshot"/>.
/// </summary>
public sealed record TreasuryStateSnapshot(
    decimal Balance,
    string Currency);
