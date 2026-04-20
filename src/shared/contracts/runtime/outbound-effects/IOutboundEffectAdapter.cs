namespace Whycespace.Shared.Contracts.Runtime.OutboundEffects;

/// <summary>
/// R3.B.1 / R-OUT-EFF-PROHIBITION-03 — the ONLY type that may house provider
/// HTTP/SDK calls. Adapters are stateless singletons (no mutable instance
/// fields) registered under their <see cref="ProviderId"/>. Retry /
/// backoff / exhaustion / classification live in <c>OutboundEffectRelay</c>,
/// NOT in adapter implementations.
///
/// <para>Prohibition summary enforced by architecture tests:</para>
/// <list type="bullet">
///   <item><c>R-OUT-EFF-PROHIBITION-03</c> — only the relay may invoke
///         <see cref="DispatchAsync"/>.</item>
///   <item><c>R-OUT-EFF-PROHIBITION-01</c> — <c>HttpClient</c> /
///         <c>HttpClientHandler</c> / <c>WebClient</c> are banned outside
///         the OPA grandfathered whitelist and
///         <c>src/platform/host/adapters/outbound-effects/**</c>.</item>
///   <item><c>R-OUT-EFF-PROHIBITION-02</c> — third-party SDK namespaces may
///         reference only inside adapter files.</item>
///   <item><c>R-OUT-EFF-DET-01</c> — adapters use
///         <see cref="Whycespace.Shared.Kernel.Domain.IClock"/> /
///         <see cref="Whycespace.Shared.Kernel.Domain.IIdGenerator"/> /
///         <see cref="Whycespace.Shared.Kernel.Domain.IRandomProvider"/>
///         only. No <c>Guid.NewGuid()</c>, no <c>DateTime.UtcNow</c>.</item>
/// </list>
/// </summary>
public interface IOutboundEffectAdapter
{
    /// <summary>Stable provider key (matches <c>OutboundEffectIntent.ProviderId</c>).</summary>
    string ProviderId { get; }

    /// <summary>Duplicate-handling shape — relay consults for post-dispatch retry safety.</summary>
    OutboundIdempotencyShape IdempotencyShape { get; }

    /// <summary>How finality arrives for this provider (Push / Poll / Hybrid / ManualOnly).</summary>
    OutboundFinalityStrategy FinalityStrategy { get; }

    /// <summary>
    /// Execute a single dispatch attempt. Adapter is stateless; retry loops
    /// live in <c>OutboundEffectRelay</c>. Return one of the six sealed
    /// <see cref="OutboundAdapterResult"/> variants — collapse is forbidden.
    /// </summary>
    Task<OutboundAdapterResult> DispatchAsync(
        OutboundEffectDispatchContext context,
        CancellationToken cancellationToken);

    /// <summary>
    /// R3.B.4 — poll provider for business finality of an earlier-acknowledged
    /// operation. REQUIRED for adapters declaring
    /// <see cref="OutboundFinalityStrategy.Poll"/> or
    /// <see cref="OutboundFinalityStrategy.Hybrid"/>; default implementation
    /// throws <see cref="NotSupportedException"/> for <c>Push</c> / <c>ManualOnly</c>
    /// adapters that have no polling surface.
    ///
    /// <para>Adapters returning <see cref="OutboundFinalityPollResult.StillPending"/>
    /// tell the poller to keep the row in <c>Acknowledged</c> status until the
    /// next poll cycle or finality-window expiry. All other variants translate
    /// to a lifecycle transition.</para>
    /// </summary>
    Task<OutboundFinalityPollResult> PollFinalityAsync(
        ProviderOperationIdentity providerOperation,
        CancellationToken cancellationToken) =>
        throw new NotSupportedException(
            $"Adapter '{ProviderId}' with FinalityStrategy={FinalityStrategy} does not support PollFinalityAsync. " +
            "Only Poll and Hybrid strategies must implement this seam.");
}
