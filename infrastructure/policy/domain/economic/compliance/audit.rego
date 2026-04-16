package whyce.policy.economic.compliance.audit

import rego.v1

default allow := false

# whyce.economic.compliance.audit.create — Operator (system-initiated on event
# observation) OR Admin (manual compliance capture).
allow if {
    input.policy_id == "whyce.economic.compliance.audit.create"
    input.subject.role == "operator"
    valid_resource
}
allow if {
    input.policy_id == "whyce.economic.compliance.audit.create"
    input.subject.role == "admin"
    valid_resource
}

# whyce.economic.compliance.audit.finalize — Operator OR Admin. Finalization
# closes the audit record lifecycle; no further mutations are permitted.
allow if {
    input.policy_id == "whyce.economic.compliance.audit.finalize"
    input.subject.role == "operator"
    valid_resource
}
allow if {
    input.policy_id == "whyce.economic.compliance.audit.finalize"
    input.subject.role == "admin"
    valid_resource
}

# whyce.economic.compliance.audit.read — Operator OR Admin (read-side access).
allow if {
    input.policy_id == "whyce.economic.compliance.audit.read"
    input.subject.role == "operator"
    valid_resource
}
allow if {
    input.policy_id == "whyce.economic.compliance.audit.read"
    input.subject.role == "admin"
    valid_resource
}

valid_resource if {
    input.resource.classification == "economic"
    input.resource.context == "compliance"
    input.resource.domain == "audit"
}

deny if { not input.policy_id }
deny if { not input.subject.role }
deny if { not input.resource.classification }
