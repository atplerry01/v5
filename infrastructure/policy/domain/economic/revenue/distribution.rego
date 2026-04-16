package whyce.policy.economic.revenue.distribution

import rego.v1

default allow := false

allow if {
    input.subject.role == "revenue-admin"
}

deny if { not input.subject.role }
