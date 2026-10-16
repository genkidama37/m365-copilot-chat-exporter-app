# 02: Self-Contained Offline HTML & Master Index Slice

Status: ready-for-agent

**Blocked by:** 01: Foundation & First Tracer Bullet: Ingest-to-Markdown Slice

## What to build
An end-to-end export capability that generates 100% self-contained offline HTML documents with speech-bubble turn styling and automatic dark/light mode adaptation, alongside a root `index.html` catalog dashboard linking to all exported conversations, with zero external CDN dependencies.

## Acceptance Criteria

- [x] **Self-Contained HTML Output**:
  - Produces valid standalone HTML5 files named `{YYYY-MM-DD} - {SanitizedTitle}.html`.
  - All CSS is embedded directly in `<style>` blocks—zero `<link rel="stylesheet">` to remote CDNs and zero external script tags.
  - Turns are rendered with distinct speech-bubble styling for user prompts and Copilot responses.
  - Pre-formatted code blocks and tables render cleanly.
  - Includes `@media (prefers-color-scheme: dark)` styling adapting automatically to operating system dark mode.
- [x] **Citation Footnotes in HTML**:
  - Citations render as interactive badges linking to bottom footnote source references with clickable hyperlinks.
- [x] **Master Index Dashboard (`index.html`)**:
  - Generates a root `index.html` summary file listing all exported conversations in a table (Title, Date, Turn Count).
  - Includes working relative hyperlinks to each conversation's `.html` and `.md` files.
  - Table includes embedded client-side search/filtering JavaScript that works offline without external libraries.
- [x] **Air-Gapped Offline Verification**:
  - Test verifies generated HTML documents contain no external URL resources (`http://` or `https://` in `<link>`, `<script>`, or `@import`) except hyperlinked citation URLs in `href`.
- [x] **Automated Seam Verification**:
  - Integration tests in `CopilotChatExporter.Core.Tests` verify HTML export generation and master index generation against test fixtures.

## Verification
Run `dotnet test --filter "FullyQualifiedName~HtmlExporterTests"` to verify self-contained HTML generation, offline assertions, and master index creation.
