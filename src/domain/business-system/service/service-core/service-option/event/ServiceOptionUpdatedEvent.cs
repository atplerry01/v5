namespace Whycespace.Domain.BusinessSystem.Service.ServiceCore.ServiceOption;

public sealed record ServiceOptionUpdatedEvent(ServiceOptionId ServiceOptionId, OptionName Name, OptionKind Kind);
