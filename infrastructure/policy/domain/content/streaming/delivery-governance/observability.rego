package whyce.policy.content.streaming.delivery_governance.observability

import rego.v1

default allow := false

allow if { input.policy_id == "whyce.content.streaming.delivery_governance.observability.capture"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.content.streaming.delivery_governance.observability.update"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.content.streaming.delivery_governance.observability.finalize"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.content.streaming.delivery_governance.observability.archive"; input.subject.role == "operator"; valid_resource }

valid_resource if { input.resource.classification in ["content", "system"] }
