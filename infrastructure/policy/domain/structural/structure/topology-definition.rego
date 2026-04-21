package whyce.policy.structural.structure.topology_definition

import rego.v1

default allow := false

allow if { input.policy_id == "whyce.structural.structure.topology_definition.create"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.structural.structure.topology_definition.activate"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.structural.structure.topology_definition.suspend"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.structural.structure.topology_definition.reactivate"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.structural.structure.topology_definition.retire"; input.subject.role == "operator"; valid_resource }

valid_resource if { input.resource.classification in ["structural", "system"] }
