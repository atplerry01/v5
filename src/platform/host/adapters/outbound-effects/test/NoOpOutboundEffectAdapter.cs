using System.Security.Cryptography;
using System.Text;
using Whycespace.Shared.Contracts.Runtime.OutboundEffects;

namespace Whycespace.Platform.Host.Adapters.OutboundEffects.Test;

/// <summary>
/// R3.B.1 / D-R3B1-4 — test-scoped adapter that proves the dispatcher → queue
/// → relay → adapter → lifecycle-event flow without committing to a real
/// provider. Deterministic: the provider operation id is a SHA-256 hash of
/// the idempotency key so replays collapse to the same identity.
///
/// <para><b>R-OUT-EFF-PROHIBITION-05 (S1):</b> this type MUST NOT be
/// registered in production composition. The
/// <c>NoOp_Adapter_Not_In_Production_Composition</c> architecture test
/// asserts zero hits for <c>NoOpOutboundEffectAdapter</c> under
/// <c>Program.cs</c> / non-test composition modules.</para>
/// </summary>
internal sealed class NoOpOutboundEffectAdapter : IOutboundEffectAdapter
{
    public string ProviderId => "noop";
    public OutboundIdempotencyShape IdempotencyShape => OutboundIdempotencyShape.ProviderIdempotent;
    public OutboundFinalityStrategy FinalityStrategy => OutboundFinalityStrategy.ManualOnly;

    public Task<OutboundAdapterResult> DispatchAsync(
        OutboundEffectDispatchContext context,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var providerOpId = DeriveProviderOperationId(context.IdempotencyKey.Value);
        var identity = new ProviderOperationIdentity(
            ProviderId: ProviderId,
            ProviderOperationId: providerOpId,
            IdempotencyKeyUsed: context.IdempotencyKey.Value);

        OutboundAdapterResult result = new OutboundAdapterResult.Acknowledged(identity);
        return Task.FromResult(result);
    }

    private static string DeriveProviderOperationId(string idempotencyKey)
    {
        var bytes = Encoding.UTF8.GetBytes(idempotencyKey);
        var digest = SHA256.HashData(bytes);
        return $"noop-{Convert.ToHexString(digest).AsSpan(0, 16)}";
    }
}
