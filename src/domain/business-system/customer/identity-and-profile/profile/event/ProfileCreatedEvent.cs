using Whycespace.Domain.BusinessSystem.Shared.Reference;

namespace Whycespace.Domain.BusinessSystem.Customer.IdentityAndProfile.Profile;

public sealed record ProfileCreatedEvent(
    ProfileId ProfileId,
    CustomerRef Customer,
    ProfileDisplayName DisplayName);
