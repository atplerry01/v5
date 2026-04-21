package whyce.policy.structural.humancapital.assignment

import rego.v1

default allow := false

allow if { input.policy_id == "whyce.structural.humancapital.assignment.assign"; input.subject.role == "operator"; valid_resource }

valid_resource if { input.resource.classification in ["structural", "system"] }
