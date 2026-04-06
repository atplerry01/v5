namespace Whycespace.Domain.StructuralSystem.Entity.EntityRegistry;

public sealed class EntityAggregate : AggregateRoot
{
    // ============================================
    // CORE CLASSIFICATION
    // ============================================

    public string EntityType { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public Guid? ParentEntityId { get; private set; }
    public bool IsActive { get; private set; }

    // ============================================
    // ROLE + CAPABILITY
    // ============================================

    private readonly EntityRoles _roles = new();
    private readonly EntityCapabilities _capabilities = new();

    public IReadOnlyCollection<EntityRole> Roles => _roles.Values;
    public IReadOnlyCollection<EntityCapability> Capabilities => _capabilities.Values;

    // ============================================
    // BINDING
    // ============================================

    private EntityBinding? _binding;
    public EntityBinding? Binding => _binding;

    // ============================================
    // RELATIONSHIPS
    // ============================================

    private readonly HashSet<EntityRelationship> _relationships = new();
    public IReadOnlyCollection<EntityRelationship> Relationships => _relationships;

    // ============================================
    // CONSTRUCTOR
    // ============================================

    public EntityAggregate() { }

    // ============================================
    // FACTORY (DETERMINISTIC CREATION)
    // ============================================

    public static EntityAggregate Register(
        Guid id,
        string entityType,
        string name,
        Guid? parentId = null)
    {
        Guard.AgainstDefault(id);
        Guard.AgainstEmpty(entityType);
        Guard.AgainstEmpty(name);

        var aggregate = new EntityAggregate
        {
            Id = id,
            EntityType = entityType,
            Name = name,
            ParentEntityId = parentId,
            IsActive = true
        };

        aggregate.RaiseDomainEvent(new EntityRegisteredEvent(id, entityType, name, parentId));

        return aggregate;
    }

    // ============================================
    // ROLE MANAGEMENT
    // ============================================

    public void AssignRole(EntityRole role)
    {
        EnsureInvariant(
            ValidRoleForEntityTypeSpecification.IsSatisfied(EntityType, role.Value),
            "VALID_ROLE_FOR_TYPE",
            $"Role '{role.Value}' is not valid for entity type '{EntityType}'");

        if (_roles.Contains(role))
            return;

        _roles.Add(role);

        foreach (var capability in RoleCapabilityMapping.Map(role))
        {
            if (!_capabilities.Contains(capability))
            {
                _capabilities.Add(capability);
                RaiseDomainEvent(new EntityCapabilityAssignedEvent(Id, capability.Value));
            }
        }

        RaiseDomainEvent(new EntityRoleAssignedEvent(Id, role.Value));
    }

    public void RemoveRole(EntityRole role)
    {
        if (!_roles.Contains(role))
            return;

        _roles.Remove(role);

        RaiseDomainEvent(new EntityRoleRemovedEvent(Id, role.Value));
    }

    // ============================================
    // BINDING OPERATIONS
    // ============================================

    public void BindToEconomic(Guid accountId)
    {
        Guard.AgainstDefault(accountId);
        EnsureInvariant(
            ValidEntityBindingSpecification.CanBindToEconomic(EntityType),
            "ECONOMIC_BINDING_ALLOWED",
            $"Entity type '{EntityType}' cannot bind to economic system");

        _binding = new EntityBinding(
            Id,
            accountId,
            _binding?.WorkflowContextId,
            _binding?.GovernanceScope,
            _binding?.IdentityId);

        RaiseDomainEvent(new EntityBoundToEconomicEvent(Id, accountId));
    }

    public void BindToWorkflow(Guid workflowContextId)
    {
        Guard.AgainstDefault(workflowContextId);
        EnsureInvariant(
            ValidEntityBindingSpecification.CanBindToWorkflow(EntityType),
            "WORKFLOW_BINDING_ALLOWED",
            $"Entity type '{EntityType}' cannot bind to workflow system");

        _binding = new EntityBinding(
            Id,
            _binding?.EconomicAccountId,
            workflowContextId,
            _binding?.GovernanceScope,
            _binding?.IdentityId);

        RaiseDomainEvent(new EntityBoundToWorkflowEvent(Id, workflowContextId));
    }

    public void BindToGovernance(string scope)
    {
        Guard.AgainstEmpty(scope);
        EnsureInvariant(
            ValidEntityBindingSpecification.CanBindToGovernance(EntityType),
            "GOVERNANCE_BINDING_ALLOWED",
            $"Entity type '{EntityType}' cannot bind to governance system");

        _binding = new EntityBinding(
            Id,
            _binding?.EconomicAccountId,
            _binding?.WorkflowContextId,
            scope,
            _binding?.IdentityId);

        RaiseDomainEvent(new EntityBoundToGovernanceEvent(Id, scope));
    }

    public void BindToIdentity(Guid identityId)
    {
        Guard.AgainstDefault(identityId);
        EnsureInvariant(
            ValidEntityBindingSpecification.CanBindToIdentity(EntityType),
            "IDENTITY_BINDING_ALLOWED",
            $"Entity type '{EntityType}' cannot bind to identity system");

        _binding = new EntityBinding(
            Id,
            _binding?.EconomicAccountId,
            _binding?.WorkflowContextId,
            _binding?.GovernanceScope,
            identityId);

        RaiseDomainEvent(new EntityBoundToIdentityEvent(Id, identityId));
    }

    // ============================================
    // RELATIONSHIP GRAPH
    // ============================================

    public void AddRelationship(Guid toEntityId, EntityRelationshipType type, string toEntityType)
    {
        Guard.AgainstDefault(toEntityId);
        EnsureInvariant(
            ValidEntityRelationshipSpecification.IsSatisfied(EntityType, toEntityType, type.Value),
            "VALID_RELATIONSHIP",
            $"Invalid relationship {type.Value} between {EntityType} and {toEntityType}");

        var relationship = new EntityRelationship(Id, toEntityId, type.Value);

        if (_relationships.Contains(relationship))
            return;

        _relationships.Add(relationship);
        RaiseDomainEvent(new EntityRelationshipCreatedEvent(Id, toEntityId, type.Value));
    }

    public void RemoveRelationship(Guid toEntityId, EntityRelationshipType type)
    {
        var relationship = new EntityRelationship(Id, toEntityId, type.Value);

        if (!_relationships.Contains(relationship))
            return;

        _relationships.Remove(relationship);
        RaiseDomainEvent(new EntityRelationshipRemovedEvent(Id, toEntityId, type.Value));
    }

    // ============================================
    // STRUCTURAL OPERATIONS
    // ============================================

    public void LinkToParent(Guid parentId)
    {
        Guard.AgainstDefault(parentId);

        if (ParentEntityId == parentId)
            return;

        ParentEntityId = parentId;
        RaiseDomainEvent(new EntityLinkedEvent(Id, parentId));
    }

    public void Reparent(Guid newParentId)
    {
        Guard.AgainstDefault(newParentId);

        if (ParentEntityId == newParentId)
            return;

        ParentEntityId = newParentId;
        RaiseDomainEvent(new EntityReparentedEvent(Id, newParentId));
    }

    // ============================================
    // LIFECYCLE
    // ============================================

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        RaiseDomainEvent(new EntityDeactivatedEvent(Id));
    }

    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        RaiseDomainEvent(new EntityActivatedEvent(Id));
    }

    // ============================================
    // INVARIANTS
    // ============================================

    public void EnsureValidState()
    {
        EnsureInvariant(
            EntityType != "SPV" || _roles.Values.Any(),
            "SPV_MUST_HAVE_ROLE",
            "SPV must have at least one role");
    }
}
