using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.TrustSystem.Identity.Trust;

public sealed record TrustAssessedEvent(TrustId TrustId, TrustDescriptor Descriptor, Timestamp AssessedAt) : DomainEvent;
