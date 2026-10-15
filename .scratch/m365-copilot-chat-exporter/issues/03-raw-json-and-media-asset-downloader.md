# 03: Raw JSON & Media Asset Downloader Slice

Status: ready-for-agent

**Blocked by:** 01: Foundation & First Tracer Bullet: Ingest-to-Markdown Slice, 02: Self-Contained Offline HTML & Master Index Slice

## What to build
An end-to-end data fidelity and asset archiving slice that produces full-fidelity raw JSON conversation dumps and downloads embedded images, diagrams, and media attachments into a local `assets/` subfolder, rewriting image paths in Markdown and HTML exports so visual content remains accessible offline.

## Acceptance Criteria

- [ ] **Raw JSON Serialization**:
  - Generates `{YYYY-MM-DD} - {SanitizedTitle}.json` preserving complete conversation structure, turn sequences, citation metadata, and raw API response fields without data loss.
- [ ] **Media Asset Discovery & Downloading**:
  - Identifies image URLs (e.g. generated charts, uploaded screenshots) referenced in turn responses.
  - Downloads binary image payloads into an `assets/` subfolder within the export bundle.
  - Generates deterministic cryptographic hash filenames (e.g. `assets/{sha256}.png`) to prevent collisions and avoid duplicate downloads.
- [ ] **Relative Link Rewriting**:
  - Markdown and HTML export generators rewrite remote image URLs to relative local paths (`assets/{hash}.png`).
- [ ] **Error Resilience**:
  - HTTP download errors or expired media tokens log a warning without aborting the export of the conversation text.
- [ ] **Automated Seam Verification**:
  - Integration tests using mock HTTP handlers verify image asset downloading, hash generation, link rewriting, and error resilience.

## Verification
Run `dotnet test --filter "FullyQualifiedName~AssetDownloaderTests"` and `dotnet test --filter "FullyQualifiedName~JsonExporterTests"`.
