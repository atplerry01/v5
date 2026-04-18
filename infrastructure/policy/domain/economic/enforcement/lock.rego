package whyce.policy.economic.enforcement.lock

import rego.v1

# Stub policy for enforcement.lock — authors allow rules per command as the
# surface matures. Mirrors the contract.rego / limit.rego patterns: deny by
# default, explicit allow branches per role.
#
# Binding covered (EnforcementPolicyModule / LockController):
#   whyce.economic.enforcement.lock.lock

default allow := false

allow if {
    input.subject.role == "admin"
}

# ops-validator — operational validation harness role (test-only).
allow if {
    input.subject.role == "ops-validator"
}

deny if { not input.subject.role }
