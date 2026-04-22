# platform-system / command / command-routing

**Classification:** platform-system  
**Context:** command  
**Domain:** command-routing

## Purpose
Routing rule binding a command type to a handler address (DomainRoute). This domain owns the routing table as a set of immutable, versioned rules.

## Scope
- Route rule registration: command type → handler DomainRoute
- Route rule deprecation when a command type is retired

## Does Not Own
- Routing execution (→ engine layer)
- Command type definitions (→ command-definition)
- Policy governing who may issue a command (→ control-system/system-policy)

## Inputs
- Command type ID, handler DomainRoute

## Outputs
- `CommandRoutingRegisteredEvent`
- `CommandRoutingRemovedEvent`

## Invariants
- Exactly one active routing rule per command type at any time
- Route rule ID is deterministic: SHA256 of (commandTypeId + handlerRoute)
- Routing rules are immutable; updates create a new rule and deprecate the prior

## Dependencies
- `core-system/identifier` — route rule ID
