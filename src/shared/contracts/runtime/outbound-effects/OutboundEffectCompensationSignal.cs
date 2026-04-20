namespace Whycespace.Shared.Contracts.Runtime.OutboundEffects;

/// <summary>
/// R3.B.5 — shared-contracts DTO projected from
/// <c>OutboundEffectCompensationRequestedEvent</c>. Handlers consume this DTO
/// instead of the raw domain event so the handler interface can live in
/// shared contracts without dragging a domain dependency into it.
///
/// <para>The DTO carries every field a caller needs to dispatch its own
/// compensation workflow — aggregate id (the outbound effect id), effect
/// type, provider id, triggering outcome, and optional caller-aggregate
/// correlation fields when the scheduling code set them.</para>
/// </summary>
public sealed record OutboundEffectCompensationSignal(
    Guid EffectId,
    string EffectType,
    string ProviderId,
    string TriggeringOutcome,
    string? OwnerAggregateType = null,
    Guid? OwnerAggregateId = null);
