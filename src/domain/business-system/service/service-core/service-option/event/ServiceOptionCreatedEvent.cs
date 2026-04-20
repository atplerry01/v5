using Whycespace.Domain.BusinessSystem.Shared.Reference;

namespace Whycespace.Domain.BusinessSystem.Service.ServiceCore.ServiceOption;

public sealed record ServiceOptionCreatedEvent(
    ServiceOptionId ServiceOptionId,
    ServiceDefinitionRef ServiceDefinition,
    OptionCode Code,
    OptionName Name,
    OptionKind Kind);
