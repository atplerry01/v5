package whyce.policy.economic.transaction.charge

import rego.v1

# Stub policy for transaction.charge — mirrors the limit.rego sibling pattern
# in the same context: deny by default, explicit allow branches per role,
# plus an `is_system` path for charges calculated by in-process workflow steps.
#
# Binding covered:
#   whyce.economic.transaction.charge.calculate

default allow := false

# System path — in-process workflow steps dispatch CalculateChargeCommand
# via DispatchSystemAsync.
allow if {
    input.is_system == true
}

allow if {
    input.subject.role in {"operator", "admin"}
}

# ops-validator — operational validation harness role (test-only).
allow if {
    input.subject.role == "ops-validator"
}

deny if { not input.subject.role }
