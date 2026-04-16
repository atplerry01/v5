package whyce.policy.economic.ledger.entry

import rego.v1

# Entry is a derivative domain: LedgerEntryAggregate instances are created as
# side effects of journal posting, not via a standalone command surface. There
# is therefore no CommandPolicyBinding for entry in LedgerPolicyModule and no
# policy_id will ever be stamped that resolves to this package for a write
# path. This package exists to satisfy PB-04 scope completeness (every ledger
# domain has a loaded rego package) and to return a deterministic deny for any
# future read-time or audit-time evaluation keyed on ledger.entry.
#
# If entries gain a standalone command surface, add matching `allow` rules
# here (mirroring journal.rego) in the same change-set that adds the
# CommandPolicyBinding — never one without the other.

default allow := false

valid_resource if {
    input.resource.classification == "economic"
    input.resource.context == "ledger"
    input.resource.domain == "entry"
}

deny if { not input.policy_id }
deny if { not input.subject.role }
deny if { not input.resource.classification }
