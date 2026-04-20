using Whycespace.Domain.BusinessSystem.Shared.Reference;

using Whycespace.Domain.BusinessSystem.Shared.Time;

namespace Whycespace.Domain.BusinessSystem.Service.ServiceConstraint.ServiceWindow;

public sealed record ServiceWindowCreatedEvent(
    ServiceWindowId ServiceWindowId,
    ServiceDefinitionRef ServiceDefinition,
    TimeWindow Range);
