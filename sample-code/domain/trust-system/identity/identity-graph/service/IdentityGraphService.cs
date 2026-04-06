namespace Whycespace.Domain.TrustSystem.Identity.IdentityGraph;

public sealed class IdentityGraphService
{
    public bool AreLinked(IdentityGraphAggregate graph, Guid identityA, Guid identityB)
        => graph.Links.Any(l =>
            l.IsActive &&
            ((l.SourceIdentityId == identityA && l.TargetIdentityId == identityB) ||
             (l.SourceIdentityId == identityB && l.TargetIdentityId == identityA)));
}
