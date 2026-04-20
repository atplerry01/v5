# localization (SCAFFOLD — pending implementation)

## Purpose

The `localization` leaf owns **locale variants** of content objects — translations of documents, dubbing/subtitle variants of media, locale-specific streaming tracks. A localization aggregate binds a source content object to its localized variant output.

## Owns

- Localization identity, source ref, target locale, variant output ref, status (Requested / InProgress / Published / Withdrawn).
- Request / start / complete / publish / withdraw transitions.

## Does not own

- The translation/dubbing engine — infrastructure.
- Subtitle/transcript aggregates — those are media-local under `media/core-object/{subtitle,transcript}`.
- User locale preferences — identity/platform concern.

## Status

SCAFFOLD only in P2.6.CS.5.0.
