using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.TrustSystem.Identity.Device;

public sealed record DeviceRegisteredEvent(DeviceId DeviceId, DeviceDescriptor Descriptor, Timestamp RegisteredAt) : DomainEvent;
