using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.TrustSystem.Identity.Device;

public sealed record DeviceDeregisteredEvent(DeviceId DeviceId) : DomainEvent;
