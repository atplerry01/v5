package whyce.policy.content.streaming.delivery_governance.access

import rego.v1

default allow := false

allow if { input.policy_id == "whyce.content.streaming.delivery_governance.access.grant"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.content.streaming.delivery_governance.access.restrict"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.content.streaming.delivery_governance.access.unrestrict"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.content.streaming.delivery_governance.access.revoke"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.content.streaming.delivery_governance.access.expire"; input.subject.role == "operator"; valid_resource }

valid_resource if { input.resource.classification in ["content", "system"] }
