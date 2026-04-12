namespace Whycespace.Domain.TrustSystem.Identity.Identity;

public sealed record IdentityEstablishedEvent(IdentityId IdentityId, IdentityDescriptor Descriptor);
