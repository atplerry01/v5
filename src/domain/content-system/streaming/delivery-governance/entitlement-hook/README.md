# entitlement-hook (SCAFFOLD — pending implementation)

## Purpose

The `entitlement-hook` leaf owns **the adapter boundary to an upstream entitlement-of-record system** (commerce / rights authority). Distinct from `access` per §CD-09: `access` owns technical access grants; `entitlement-hook` asks the upstream authority whether a commercial/rights entitlement exists.

## Owns (planned)

- Entitlement-hook identity, source system ref, target ref (stream / channel / archive), query result cache, last-checked timestamp, status (Unknown / Entitled / NotEntitled / Expired / Error).
- Query / refresh / invalidate / record-failure transitions.

## Does not own

- The upstream entitlement-of-record (that lives outside content-system).
- The actual entitlement grant (this domain only RECORDS the upstream answer).
- Technical access enforcement — that is `access`.

## §CD-09 disambiguation

- `access/` = **technical** gating (token, window, mode) on a stream.
- `entitlement-hook/` = **commercial/rights** gating adapter.
- An access grant may depend on an entitlement-hook response, but they are
  two distinct lifecycles.

## Status

SCAFFOLD only in P2.6.CS.7.
