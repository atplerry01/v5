package whyce.policy.economic.exchange.rate

import rego.v1

# Default-safe: every action is denied unless an explicit allow rule matches.
default allow := false

# whyce.economic.exchange.rate.define
# Operator defines a new rate for an fx pair.
allow if {
    input.policy_id == "whyce.economic.exchange.rate.define"
    input.subject.role == "operator"
    valid_resource
}
allow if {
    input.policy_id == "whyce.economic.exchange.rate.define"
    input.subject.role == "admin"
    valid_resource
}

# whyce.economic.exchange.rate.activate
allow if {
    input.policy_id == "whyce.economic.exchange.rate.activate"
    input.subject.role == "operator"
    valid_resource
}
allow if {
    input.policy_id == "whyce.economic.exchange.rate.activate"
    input.subject.role == "admin"
    valid_resource
}

# whyce.economic.exchange.rate.expire — admin or operator (immutability boundary).
allow if {
    input.policy_id == "whyce.economic.exchange.rate.expire"
    input.subject.role == "admin"
    valid_resource
}
allow if {
    input.policy_id == "whyce.economic.exchange.rate.expire"
    input.subject.role == "operator"
    valid_resource
}

# Resource binding — every exchange.rate allow path requires correct route.
valid_resource if {
    input.resource.classification == "economic"
    input.resource.context == "exchange"
    input.resource.domain == "rate"
}

# Hard denies — surface missing inputs as policy violations rather than
# silently failing closed.
deny if { not input.policy_id }
deny if { not input.subject.role }
deny if { not input.resource.classification }
