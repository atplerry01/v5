# media / companion-artifact

## Purpose

Groups artifacts that exist to **accompany** a media asset rather than to be the asset itself: subtitle and transcript.

## Why this group exists

Subtitles and transcripts are not primary media. They depend on a media asset, carry their own lifecycle, and have their own typed value objects (format, language, timing window). Collapsing them into `content-artifact/` would blur the semantic distinction between primary artifacts and accompaniments. Grouping them into `companion-artifact/` keeps that distinction explicit and leaves room for future companion artifacts (e.g. chapter markers, alternate audio tracks) without renaming.

## Leaf domains

- `subtitle/` — subtitle companion artifact (format, language, timing window, output ref, draft → active → finalized → archived).
- `transcript/` — transcript companion artifact (format, language, output ref, draft → active → finalized → archived).

## Boundary notes

- Both hold `MediaAssetRef` back-pointers; they do not own the media asset.
- Both may optionally reference the source `MediaFileRef` they were derived from.
- They do not own the rendering pipeline that produced them (ASR, OCR, translation) — those are infrastructure adapters driven from `media/lifecycle/processing`.
