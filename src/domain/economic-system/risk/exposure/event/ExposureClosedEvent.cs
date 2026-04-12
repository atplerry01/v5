using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Risk.Exposure;

public sealed record ExposureClosedEvent(
    ExposureId ExposureId) : DomainEvent;
