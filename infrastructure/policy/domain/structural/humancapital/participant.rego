package whyce.policy.structural.humancapital.participant

import rego.v1

default allow := false

allow if { input.policy_id == "whyce.structural.humancapital.participant.register"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.structural.humancapital.participant.place"; input.subject.role == "operator"; valid_resource }

valid_resource if { input.resource.classification in ["structural", "system"] }
