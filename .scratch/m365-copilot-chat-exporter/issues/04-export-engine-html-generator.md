# Export Engine HTML Generator and Master Index

Status: ready-for-agent

## Description

Implement the 100% self-contained offline HTML generator and master catalog index generator in `CopilotChatExporter.Core`.

## Scope of Work

1. Implement `IHtmlExporter`:
   - Generate standalone `.html` files for individual conversations (`{YYYY-MM-DD} - {SanitizedTitle}.html`).
   - Embed responsive CSS directly into `<style>` tags with zero external CDN dependencies (no remote fonts or external scripts).
   - Support modern UI presentation: speech bubble layout for turns, clear metadata badges, and code blocks.
   - Include dark/light mode adaptation using CSS `@media (prefers-color-scheme: dark)`.
   - Render citations as numbered references linking to footnotes with source URLs.
2. Implement `IIndexCatalogGenerator`:
   - Generate a root `index.html` file summarizing all exported conversations.
   - Include an interactive table with conversation title, date, turn count, and links to corresponding `.html` and `.md` files.
   - Embed lightweight inline JavaScript for client-side table sorting and instant keyword filtering without external dependencies.
3. Unit tests verifying DOM structure, offline self-containment, and escaping of user-provided content.

## Acceptance Criteria

- Generated HTML files render correctly when opened directly in any browser with the network disabled (offline/air-gapped mode).
- Master `index.html` allows navigation across all exported conversation files.
- All styles and logic are fully self-contained.

## Comments
