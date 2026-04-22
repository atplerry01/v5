using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.TrustSystem.Identity.Profile;

public sealed record ProfileActivatedEvent(ProfileId ProfileId) : DomainEvent;
