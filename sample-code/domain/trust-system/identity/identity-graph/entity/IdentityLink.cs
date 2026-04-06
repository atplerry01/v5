using Whycespace.Shared.Primitives.Id;

namespace Whycespace.Domain.TrustSystem.Identity.IdentityGraph;

public sealed class IdentityLink : Entity
{
    public Guid GraphId { get; private set; }
    public Guid SourceIdentityId { get; private set; }
    public Guid TargetIdentityId { get; private set; }
    public LinkType LinkType { get; private set; } = null!;
    public LinkStrength Strength { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private IdentityLink() { }

    public static IdentityLink Create(
        Guid graphId, Guid sourceIdentityId, Guid targetIdentityId,
        LinkType linkType, LinkStrength strength, DateTimeOffset timestamp)
    {
        return new IdentityLink
        {
            Id = DeterministicIdHelper.FromSeed($"IdentityLink:{graphId}:{sourceIdentityId}:{targetIdentityId}"),
            GraphId = graphId,
            SourceIdentityId = sourceIdentityId,
            TargetIdentityId = targetIdentityId,
            LinkType = linkType,
            Strength = strength,
            IsActive = true,
            CreatedAt = timestamp
        };
    }

    public void Deactivate() => IsActive = false;
}
