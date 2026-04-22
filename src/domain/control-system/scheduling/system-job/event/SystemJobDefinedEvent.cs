using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Scheduling.SystemJob;

public sealed record SystemJobDefinedEvent(SystemJobId Id, string Name, JobCategory Category, TimeSpan Timeout) : DomainEvent;
