package whyce.policy.structural.structure.classification

import rego.v1

default allow := false

allow if { input.policy_id == "whyce.structural.structure.classification.define"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.structural.structure.classification.activate"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.structural.structure.classification.deprecate"; input.subject.role == "operator"; valid_resource }

valid_resource if { input.resource.classification in ["structural", "system"] }
