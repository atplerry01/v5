namespace Whycespace.Domain.BusinessSystem.Service.ServiceConstraint.PolicyBinding;

public sealed record PolicyBindingUnboundEvent(PolicyBindingId PolicyBindingId, DateTimeOffset UnboundAt);
