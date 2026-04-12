namespace Whycespace.Domain.StructuralSystem.Cluster.Subcluster;

public sealed record SubclusterDefinedEvent(SubclusterId SubclusterId, SubclusterDescriptor Descriptor);

public sealed record SubclusterActivatedEvent(SubclusterId SubclusterId);

public sealed record SubclusterArchivedEvent(SubclusterId SubclusterId);
