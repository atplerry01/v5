using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.TrustSystem.Identity.Profile;

public sealed record ProfileDeactivatedEvent(ProfileId ProfileId) : DomainEvent;
