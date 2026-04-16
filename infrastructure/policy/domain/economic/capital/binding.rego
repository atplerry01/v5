package whyce.policy.economic.capital.binding

import rego.v1

default allow := false

# whyce.economic.capital.binding.bind
# Operator only — binding established by the system at account-open / KYC pass.
allow if {
    input.policy_id == "whyce.economic.capital.binding.bind"
    input.subject.role == "operator"
    input.subject.kyc_attestation_present == true
    valid_resource
}

# whyce.economic.capital.binding.transfer
# Owner with dual consent (current + new) AND new owner trust >= floor.
# Admin override path requires a judicial / compliance flag.
allow if {
    input.policy_id == "whyce.economic.capital.binding.transfer"
    input.subject.role == "owner"
    input.subject.dual_consent_present == true
    input.subject.new_owner_trust_score >= trust_floor
    valid_resource
}
allow if {
    input.policy_id == "whyce.economic.capital.binding.transfer"
    input.subject.role == "admin"
    input.subject.compliance_override == true
    valid_resource
}

# whyce.economic.capital.binding.release
# Admin only — closure event must be present.
allow if {
    input.policy_id == "whyce.economic.capital.binding.release"
    input.subject.role == "admin"
    input.resource.closure_event_present == true
    valid_resource
}

# Configurable trust floor for binding transfers (anti-bot baseline).
# Override at deployment via OPA data input; default mirrors the project
# anti-bot baseline trust threshold.
trust_floor := 60

valid_resource if {
    input.resource.classification == "economic"
    input.resource.context == "capital"
    input.resource.domain == "binding"
}

deny if { not input.policy_id }
deny if { not input.subject.role }
deny if { not input.resource.classification }
