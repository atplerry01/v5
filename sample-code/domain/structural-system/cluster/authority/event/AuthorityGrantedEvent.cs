using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.StructuralSystem.Cluster.Authority;

public sealed record AuthorityGrantedEvent(Guid AuthorityId) : DomainEvent;
