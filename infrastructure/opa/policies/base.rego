package whyce.policy

default allow = false

allow if {
    input.subject.role == "admin"
}
