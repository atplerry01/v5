# preview (SCAFFOLD — pending implementation)

## Purpose

The `preview` leaf owns **preview renderings** of a media asset — short clips, GIFs, teaser frames. Separate from `rendition/` (which is a full encoded variant) and from `artwork/` (static images).

## Owns (planned)

- Preview identity, asset ref, preview kind (ShortClip / GifLoop / TeaserFrame), output ref, status.
- Request / render / publish / retire transitions.

## Status

SCAFFOLD only in P2.6.CS.10.
