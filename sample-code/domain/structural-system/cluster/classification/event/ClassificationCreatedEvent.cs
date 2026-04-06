using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.StructuralSystem.Cluster.Classification;

public sealed record ClassificationCreatedEvent(Guid ClassificationId) : DomainEvent;
