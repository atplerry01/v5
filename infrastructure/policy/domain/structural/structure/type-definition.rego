package whyce.policy.structural.structure.type_definition

import rego.v1

default allow := false

allow if { input.policy_id == "whyce.structural.structure.type_definition.define"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.structural.structure.type_definition.activate"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.structural.structure.type_definition.retire"; input.subject.role == "operator"; valid_resource }

valid_resource if { input.resource.classification in ["structural", "system"] }
