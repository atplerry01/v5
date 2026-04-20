# media / descriptor

## Purpose

Groups descriptive metadata attached to a media asset. Descriptor aggregates hold typed key/value entries that describe the asset without being the asset itself.

## Why this group exists

Metadata is a distinct semantic class from primary media artifacts, companion artifacts, and lifecycle aggregates. It describes a media asset but is neither the asset nor a lifecycle transition over it. Isolating it into `descriptor/` keeps the separation clean and leaves room for future descriptor-class domains (e.g. tagging, taxonomy attachment).

## Leaf domains

- `metadata/` — typed key/value metadata entries attached to a media asset, with add / update / remove / finalize. Also hosts canonical media-descriptor value objects (Bitrate, CodecName, Dimensions, Duration, LanguageTag).

## Boundary notes

- Does not own the media asset itself — only its descriptor entries. Holds a `MediaAssetRef` back-pointer.
- Does not own search indexing, faceted discovery, or catalogue presentation — those are downstream concerns.
- Once finalized, metadata is immutable (enforced by the aggregate).
