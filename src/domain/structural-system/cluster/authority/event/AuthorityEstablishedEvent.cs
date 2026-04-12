namespace Whycespace.Domain.StructuralSystem.Cluster.Authority;

public sealed record AuthorityEstablishedEvent(AuthorityId AuthorityId, AuthorityDescriptor Descriptor);

public sealed record AuthorityActivatedEvent(AuthorityId AuthorityId);

public sealed record AuthorityRevokedEvent(AuthorityId AuthorityId);
