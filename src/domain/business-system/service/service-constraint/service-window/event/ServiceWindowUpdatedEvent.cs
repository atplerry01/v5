using Whycespace.Domain.BusinessSystem.Shared.Time;

namespace Whycespace.Domain.BusinessSystem.Service.ServiceConstraint.ServiceWindow;

public sealed record ServiceWindowUpdatedEvent(ServiceWindowId ServiceWindowId, TimeWindow Range);
