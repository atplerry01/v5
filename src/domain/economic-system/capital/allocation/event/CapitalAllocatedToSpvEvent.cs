using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.StructuralSystem.Contracts.References;

namespace Whycespace.Domain.EconomicSystem.Capital.Allocation;

public sealed record CapitalAllocatedToSpvEvent(
    string AllocationId,
    string TargetId,
    decimal OwnershipPercentage) : DomainEvent
{
    /// Typed accessor over <see cref="TargetId"/>. Internal / in-memory only —
    /// never serialized to the wire. Null when the persisted TargetId is not a
    /// valid non-empty Guid (legacy data tolerated for replay).
    [JsonIgnore]
    public SpvRef? TargetSpv =>
        Guid.TryParse(TargetId, out var g) && g != Guid.Empty
            ? new SpvRef(g)
            : null;
}
