package whyce.policy.structural.structure.hierarchy_definition

import rego.v1

default allow := false

allow if { input.policy_id == "whyce.structural.structure.hierarchy_definition.define"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.structural.structure.hierarchy_definition.validate"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.structural.structure.hierarchy_definition.lock"; input.subject.role == "operator"; valid_resource }

valid_resource if { input.resource.classification in ["structural", "system"] }
