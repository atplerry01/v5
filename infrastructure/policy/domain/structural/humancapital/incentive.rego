package whyce.policy.structural.humancapital.incentive

import rego.v1

default allow := false

allow if { input.policy_id == "whyce.structural.humancapital.incentive.create"; input.subject.role == "operator"; valid_resource }

valid_resource if { input.resource.classification in ["structural", "system"] }
