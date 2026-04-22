using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.DecisionSystem.Risk.Exposure;

public sealed record ExposureClosedEvent(
    ExposureId ExposureId) : DomainEvent;
