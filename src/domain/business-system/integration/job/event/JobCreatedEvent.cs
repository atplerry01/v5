namespace Whycespace.Domain.BusinessSystem.Integration.Job;

public sealed record JobCreatedEvent(JobId JobId, JobDescriptor Descriptor);
