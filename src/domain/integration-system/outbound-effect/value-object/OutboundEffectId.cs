namespace Whycespace.Domain.IntegrationSystem.OutboundEffect;

/// <summary>
/// R3.B.1 — strongly-typed aggregate id for
/// <see cref="OutboundEffectAggregate"/>. Mirrors the
/// <c>WorkflowExecutionId</c> precedent.
/// </summary>
public readonly record struct OutboundEffectId(Guid Value);
