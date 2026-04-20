# streaming / control

## Purpose

Groups technical control surfaces over a stream. Currently houses `access` — the technical access-grant aggregate that governs whether a consumer's token is permitted to receive a stream.

## Why this group exists

Stream control is a distinct semantic class from stream identity (`stream-core/`), delivery outputs (`delivery-artifact/`), and observability (`persistence-and-observability/`). It represents **decisions applied to a stream** — grant, restrict, revoke, expire.

The group is kept as a named domain-group (per DS-R3a §3) because it represents a defined semantic category that is documented to admit future control siblings (e.g. ingest control, playback control, redirection control) as the context grows. If `access` remained the only control domain indefinitely, that would be a drift signal to revisit.

## Leaf domains

- `access/` — technical stream-access grant lifecycle (Granted / Restricted / Revoked / Expired) with mode, window, and token binding.

## Boundary notes

- `control/access` models **technical access truth**, not commercial entitlement. Whether a user has paid, subscribed, or is legally permitted is a separate concern; this aggregate only expresses that a technically valid grant exists with a given window and token binding.
- Identity of the grantee is not modelled here — the grant holds an opaque `TokenBinding` value object rather than a user reference.
- Policy-engine decisions that lead to grant/restrict/revoke actions live in the policy layer; this aggregate only records the result.
