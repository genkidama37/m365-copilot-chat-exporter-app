# 06: Interactive Catalog, FTS5 Search, Filtering & Turn Preview Slice

Status: ready-for-agent

**Blocked by:** 01: Foundation & First Tracer Bullet: Ingest-to-Markdown Slice, 05: Live Substrate API Sync Engine & Rate-Limit Backoff Slice

## What to build
An end-to-end user catalog experience in Avalonia UI that displays the list of indexed conversations with export badges, provides instant full-text search across user prompts and Copilot responses via SQLite FTS5, date range filtering, split-pane turn preview with formatted Markdown and citation links, and selection controls ("Select All", "Select Unexported").

## Acceptance Criteria

- [ ] **Virtualized Catalog View**:
  - Displays conversations in a virtualized list showing Title, Date, Turn Count, and Last Exported status badge.
- [ ] **Instant FTS5 Full-Text Search**:
  - Search input queries the SQLite FTS5 virtual table across both prompts and responses.
  - Debounced input (300ms) prevents UI stuttering and unnecessary database queries.
  - Matches update the catalog view immediately.
- [ ] **Date-Range Filtering**:
  - Date pickers filter the conversation list to the specified date interval.
- [ ] **Turn Preview Panel**:
  - Selecting a conversation loads its dialogue turns in a split-pane preview.
  - Renders user prompts in distinct bubbles and Copilot responses with formatted Markdown.
  - Citations are displayed as badges; clicking a citation launches the source document in the default OS browser.
- [ ] **Multi-Selection Controls**:
  - Checkboxes support individual selection, "Select All", "Select None", and "Select Unexported".
  - Selection count is displayed in real-time in the UI header.
- [ ] **Automated Seam Verification**:
  - ViewModel tests verify search debouncing, selection state consistency, and date filtering logic.
  - FTS5 integration tests verify query accuracy across complex multiline prompts and responses.

## Verification
Run `dotnet test --filter "FullyQualifiedName~CatalogViewModelTests"` and `dotnet test --filter "FullyQualifiedName~Fts5SearchTests"`.
