using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.StructuralSystem.Cluster.Authority;

public sealed record AuthorityCreatedEvent(Guid AuthorityId) : DomainEvent;
