package whyce.policy.content.media.descriptor.metadata

import rego.v1

default allow := false

allow if { input.policy_id == "whyce.content.media.descriptor.metadata.create"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.content.media.descriptor.metadata.add_entry"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.content.media.descriptor.metadata.update_entry"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.content.media.descriptor.metadata.remove_entry"; input.subject.role == "operator"; valid_resource }
allow if { input.policy_id == "whyce.content.media.descriptor.metadata.finalize"; input.subject.role == "operator"; valid_resource }

valid_resource if { input.resource.classification in ["content", "system"] }
