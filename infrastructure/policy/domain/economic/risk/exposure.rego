package whyce.policy.economic.risk.exposure

import rego.v1

# Stub policy for risk.exposure — mirrors the contract.rego / limit.rego
# patterns: deny by default, explicit allow branches per role.
#
# Binding covered:
#   whyce.economic.risk.exposure.create

default allow := false

allow if {
    input.subject.role in {"operator", "admin"}
}

# ops-validator — operational validation harness role (test-only).
allow if {
    input.subject.role == "ops-validator"
}

deny if { not input.subject.role }
